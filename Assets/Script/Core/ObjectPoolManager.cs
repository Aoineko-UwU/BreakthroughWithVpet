using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用对象池管理类
/// - 按预制体复用短生命周期对象，减少重复实例化和销毁带来的开销。
/// </summary>
public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    #region 池数据

    /// <summary>按预制体保存的闲置实例队列。</summary>
    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    /// <summary>按实例反查所属预制体，供对象主动归还对象池。</summary>
    private readonly Dictionary<GameObject, GameObject> instanceSources = new();

    /// <summary>闲置对象统一挂载的隐藏父节点。</summary>
    private Transform poolRoot;

    #endregion

    #region 生命周期与实例获取

    /// <summary>对象池管理器跨场景保留，供各表现系统共享。</summary>
    protected override bool PersistAcrossScenes => true;

    /// <summary>初始化闲置对象父节点。</summary>
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        GameObject root = new GameObject("PooledObjects");
        root.transform.SetParent(transform);
        poolRoot = root.transform;
    }

    /// <summary>
    /// 获取现有对象池；不存在时创建一个跨场景对象池管理器。
    /// </summary>
    /// <returns>可用的对象池管理器实例。</returns>
    public static ObjectPoolManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        ObjectPoolManager existing = FindObjectOfType<ObjectPoolManager>();
        if (existing != null)
            return existing;

        return new GameObject(nameof(ObjectPoolManager)).AddComponent<ObjectPoolManager>();
    }

    #endregion

    #region 对象获取与归还

    /// <summary>
    /// 从指定预制体的对象池获取实例；首次使用时按需创建对象。
    /// </summary>
    /// <param name="prefab">需要复用的预制体。</param>
    /// <param name="position">实例的世界坐标。</param>
    /// <param name="rotation">实例的世界旋转。</param>
    /// <param name="parent">实例启用后的父节点。</param>
    /// <returns>已经启用的对象实例；预制体为空时返回null。</returns>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
            return null;

        if (!pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools[prefab] = pool;
        }

        GameObject instance = null;
        while (pool.Count > 0 && instance == null)
            instance = pool.Dequeue();

        if (instance == null)
        {
            instance = Instantiate(prefab);
            instanceSources[instance] = prefab;
        }

        Transform instanceTransform = instance.transform;
        instanceTransform.SetParent(parent);
        instanceTransform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        return instance;
    }

    /// <summary>
    /// 将实例归还到所属对象池；不是由对象池创建的对象不会被处理。
    /// </summary>
    /// <param name="instance">需要归还的对象实例。</param>
    /// <returns>实例属于当前对象池并成功归还时为true。</returns>
    public bool Release(GameObject instance)
    {
        if (instance == null || !instanceSources.TryGetValue(instance, out GameObject prefab))
            return false;

        if (!pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools[prefab] = pool;
        }

        instance.SetActive(false);
        instance.transform.SetParent(poolRoot);
        pool.Enqueue(instance);
        return true;
    }

    /// <summary>
    /// 延迟将实例归还对象池；实例已提前归还时不会重复入队。
    /// </summary>
    /// <param name="instance">需要延迟归还的对象实例。</param>
    /// <param name="delay">延迟时间，单位为秒。</param>
    public void ReleaseAfter(GameObject instance, float delay)
    {
        if (instance != null)
            StartCoroutine(ReleaseAfterRoutine(instance, delay));
    }

    /// <summary>等待指定时长后执行对象归还。</summary>
    /// <param name="instance">需要归还的对象实例。</param>
    /// <param name="delay">等待时长，单位为秒。</param>
    /// <returns>控制延迟归还的协程迭代器。</returns>
    private IEnumerator ReleaseAfterRoutine(GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        Release(instance);
    }

    /// <summary>
    /// 尝试通过全局对象池归还实例，供对象自身的生命周期脚本调用。
    /// </summary>
    /// <param name="instance">需要归还的对象实例。</param>
    /// <returns>对象池存在且成功归还时为true。</returns>
    public static bool TryRelease(GameObject instance)
    {
        return Instance != null && Instance.Release(instance);
    }

    #endregion
}

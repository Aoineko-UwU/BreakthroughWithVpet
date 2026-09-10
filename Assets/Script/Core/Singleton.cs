using UnityEngine;

/// <summary>
/// 单例基类
/// - 提供组件单例注册、重复对象清理和可选的跨场景保留能力。
/// </summary>
/// <typeparam name="T">需要注册为单例的组件类型。</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    #region 单例访问与生命周期配置

    /// <summary>当前已注册的单例组件实例；注册前或销毁后可能为空。</summary>
    public static T Instance { get; private set; }

    /// <summary>是否在切换场景时保留当前单例对象。</summary>
    protected virtual bool PersistAcrossScenes => false;

    #endregion

    #region 注册与销毁

    /// <summary>
    /// 注册当前实例，销毁重复单例对象，并按派生类配置决定是否跨场景保留。
    /// </summary>
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;

        if (PersistAcrossScenes)
            DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 仅在销毁对象仍是当前实例时清空单例引用。
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    #endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Addressables资源管理类
/// - 统一负责资源加载、按地址缓存、按Label预加载以及句柄释放。
/// </summary>
public sealed class AddressableManager : Singleton<AddressableManager>
{
    #region 预加载配置与缓存

    /// <summary>进入游戏后需要预加载的Addressables标签。</summary>
    [Tooltip("进入游戏时预加载并缓存的Addressables标签，例如 Audio。")]
    [SerializeField] private List<string> preloadLabels = new() { "Audio" };

    /// <summary>按资源主键缓存已经加载完成的资源结果。</summary>
    private readonly Dictionary<string, UnityEngine.Object> cachedResults = new();

    /// <summary>按资源主键缓存对应的资源句柄，管理器销毁时统一释放。</summary>
    private readonly Dictionary<string, AsyncOperationHandle> cachedHandles = new();

    /// <summary>按资源主键合并并发加载请求，避免同一资源重复发起加载。</summary>
    private readonly Dictionary<string, List<Action<UnityEngine.Object>>> pendingLoads = new();

    /// <summary>已经完成预加载的标签。</summary>
    private readonly HashSet<string> preloadedLabels = new();

    /// <summary>正在执行预加载的标签。</summary>
    private readonly HashSet<string> loadingLabels = new();

    /// <summary>同一标签预加载期间等待结果的回调。</summary>
    private readonly Dictionary<string, List<Action<bool>>> pendingLabelLoads = new();

    #endregion

    #region 生命周期与实例获取

    /// <summary>该管理器跨场景保留，供所有需要Addressables资源的管理器共享。</summary>
    protected override bool PersistAcrossScenes => true;

    /// <summary>
    /// 注册全局实例并启动Inspector中配置的标签预加载。
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        foreach (string label in preloadLabels)
        {
            if (!string.IsNullOrWhiteSpace(label))
                BeginPreloadLabel(label, null);
        }
    }

    /// <summary>
    /// 获取现有管理器；不存在时在指定宿主下创建跨场景管理器。
    /// </summary>
    /// <param name="host">创建管理器时使用的宿主对象；为空时创建独立对象。</param>
    /// <returns>可用的Addressables管理器实例。</returns>
    public static AddressableManager GetOrCreate(GameObject host = null)
    {
        if (Instance != null)
            return Instance;

        AddressableManager existing = FindObjectOfType<AddressableManager>();
        if (existing != null)
            return existing;

        GameObject managerObject = host != null ? host : new GameObject(nameof(AddressableManager));
        return managerObject.AddComponent<AddressableManager>();
    }

    /// <summary>
    /// 释放所有仍有效的资源句柄，再清理单例引用。
    /// </summary>
    protected override void OnDestroy()
    {
        foreach (AsyncOperationHandle handle in cachedHandles.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        cachedHandles.Clear();
        cachedResults.Clear();
        pendingLoads.Clear();
        pendingLabelLoads.Clear();
        loadingLabels.Clear();
        preloadedLabels.Clear();
        base.OnDestroy();
    }

    #endregion

    #region 地址加载与缓存

    /// <summary>
    /// 获取已缓存的指定类型资源；缓存不存在或类型不匹配时返回false。
    /// </summary>
    /// <typeparam name="T">资源类型。</typeparam>
    /// <param name="key">资源地址或主键。</param>
    /// <param name="result">缓存中的资源结果。</param>
    /// <returns>找到指定类型的缓存资源时为true。</returns>
    public bool TryGetCached<T>(string key, out T result) where T : UnityEngine.Object
    {
        result = null;
        if (string.IsNullOrWhiteSpace(key) || !cachedResults.TryGetValue(key, out UnityEngine.Object cachedResult))
            return false;

        result = cachedResult as T;
        return result != null;
    }

    /// <summary>
    /// 优先从缓存返回资源；缓存未命中时异步加载，并合并相同资源的并发请求。
    /// </summary>
    /// <typeparam name="T">资源类型。</typeparam>
    /// <param name="key">资源地址、GUID或其他Addressables主键。</param>
    /// <param name="onLoaded">加载成功后的回调；失败或参数无效时不调用。</param>
    public void LoadAsset<T>(string key, Action<T> onLoaded) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(key) || onLoaded == null)
            return;

        if (TryGetCached(key, out T cachedResult))
        {
            onLoaded(cachedResult);
            return;
        }

        if (pendingLoads.TryGetValue(key, out List<Action<UnityEngine.Object>> callbacks))
        {
            callbacks.Add(result => InvokeTypedCallback(result, onLoaded, key));
            return;
        }

        pendingLoads[key] = new List<Action<UnityEngine.Object>>
        {
            result => InvokeTypedCallback(result, onLoaded, key)
        };

        AsyncOperationHandle<UnityEngine.Object> handle = Addressables.LoadAssetAsync<UnityEngine.Object>(key);
        handle.Completed += completedHandle =>
        {
            if (!pendingLoads.TryGetValue(key, out List<Action<UnityEngine.Object>> pendingCallbacks))
                return;

            pendingLoads.Remove(key);

            if (completedHandle.Status != AsyncOperationStatus.Succeeded || completedHandle.Result == null)
            {
                Debug.LogError($"Addressables资源加载失败 [{key}]: {completedHandle.OperationException}");
                if (completedHandle.IsValid())
                    Addressables.Release(completedHandle);
                return;
            }

            CacheAsset(key, completedHandle.Result, completedHandle);
            foreach (Action<UnityEngine.Object> callback in pendingCallbacks)
                callback(completedHandle.Result);
        };
    }

    /// <summary>
    /// 缓存资源结果和句柄；并发加载同一资源时只保留先完成的句柄并释放重复句柄。
    /// </summary>
    /// <param name="key">资源主键。</param>
    /// <param name="result">资源加载结果。</param>
    /// <param name="handle">资源加载句柄。</param>
    private void CacheAsset(string key, UnityEngine.Object result, AsyncOperationHandle handle)
    {
        if (cachedHandles.TryGetValue(key, out AsyncOperationHandle existingHandle) && existingHandle.IsValid())
        {
            if (handle.IsValid())
                Addressables.Release(handle);
            return;
        }

        cachedResults[key] = result;
        cachedHandles[key] = handle;
    }

    /// <summary>将资源结果转换为调用方要求的类型，并在类型不匹配时输出诊断。</summary>
    /// <typeparam name="T">调用方要求的资源类型。</typeparam>
    /// <param name="result">Addressables返回的资源结果。</param>
    /// <param name="callback">类型转换成功后的调用方回调。</param>
    /// <param name="key">用于日志定位的资源主键。</param>
    private static void InvokeTypedCallback<T>(UnityEngine.Object result, Action<T> callback, string key) where T : UnityEngine.Object
    {
        if (result is T typedResult)
        {
            callback(typedResult);
            return;
        }

        Debug.LogError($"Addressables资源类型不匹配 [{key}]，期望 {typeof(T).Name}，实际 {result?.GetType().Name}");
    }

    #endregion

    #region Label预加载

    /// <summary>
    /// 预加载指定Label下的全部资源，并缓存每个资源的Handle和Result。
    /// </summary>
    /// <param name="label">Addressables标签名称。</param>
    /// <param name="onCompleted">预加载结束回调；参数表示是否全部成功。</param>
    public void PreloadLabel(string label, Action<bool> onCompleted = null)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            onCompleted?.Invoke(false);
            return;
        }

        BeginPreloadLabel(label, onCompleted);
    }

    /// <summary>判断指定Label是否已经完成过预加载。</summary>
    /// <param name="label">Addressables标签名称。</param>
    /// <returns>标签已完成预加载时为true。</returns>
    public bool IsLabelPreloaded(string label)
    {
        return !string.IsNullOrWhiteSpace(label) && preloadedLabels.Contains(label);
    }

    /// <summary>登记标签预加载请求，并合并同一标签的并发回调。</summary>
    /// <param name="label">需要预加载的Addressables标签。</param>
    /// <param name="onCompleted">预加载结束回调。</param>
    private void BeginPreloadLabel(string label, Action<bool> onCompleted)
    {
        if (preloadedLabels.Contains(label))
        {
            onCompleted?.Invoke(true);
            return;
        }

        if (loadingLabels.Contains(label))
        {
            if (onCompleted != null)
                pendingLabelLoads[label].Add(onCompleted);
            return;
        }

        loadingLabels.Add(label);
        pendingLabelLoads[label] = new List<Action<bool>>();
        if (onCompleted != null)
            pendingLabelLoads[label].Add(onCompleted);
        StartCoroutine(PreloadLabelRoutine(label));
    }

    /// <summary>
    /// 查找Label资源位置，逐个异步加载并保留每个资源的句柄和结果。
    /// </summary>
    /// <param name="label">需要预加载的Addressables标签。</param>
    /// <returns>控制Label查询与资源加载过程的协程迭代器。</returns>
    private IEnumerator PreloadLabelRoutine(string label)
    {
        AsyncOperationHandle<IList<IResourceLocation>> locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(UnityEngine.Object));
        yield return locationsHandle;

        bool success = locationsHandle.Status == AsyncOperationStatus.Succeeded;
        if (success)
        {
            foreach (IResourceLocation location in locationsHandle.Result)
            {
                AsyncOperationHandle<UnityEngine.Object> assetHandle = Addressables.LoadAssetAsync<UnityEngine.Object>(location);
                yield return assetHandle;

                if (assetHandle.Status == AsyncOperationStatus.Succeeded && assetHandle.Result != null)
                {
                    string key = location.PrimaryKey;
                    CacheAsset(key, assetHandle.Result, assetHandle);
                }
                else
                {
                    success = false;
                    if (assetHandle.IsValid())
                        Addressables.Release(assetHandle);
                    Debug.LogError($"Addressables Label资源加载失败 [{label}/{location.PrimaryKey}]: {assetHandle.OperationException}");
                }
            }
        }
        else
        {
            Debug.LogError($"Addressables Label查询失败 [{label}]: {locationsHandle.OperationException}");
        }

        if (locationsHandle.IsValid())
            Addressables.Release(locationsHandle);

        loadingLabels.Remove(label);
        if (success)
            preloadedLabels.Add(label);

        if (pendingLabelLoads.TryGetValue(label, out List<Action<bool>> callbacks))
        {
            pendingLabelLoads.Remove(label);
            foreach (Action<bool> callback in callbacks)
                callback(success);
        }
    }

    #endregion
}

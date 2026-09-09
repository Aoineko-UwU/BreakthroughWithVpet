using UnityEngine;

/// <summary>
/// 为场景组件提供统一的单例实例、重复对象清理和可选的跨场景持久化行为。
/// </summary>
/// <typeparam name="T">单例组件的具体类型。</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// 当前场景中有效的单例实例。
    /// </summary>
    public static T Instance { get; private set; }

    /// <summary>
    /// 是否在切换场景时保留该单例对象。
    /// </summary>
    protected virtual bool PersistAcrossScenes => false;

    /// <summary>
    /// 注册单例并清理重复对象。
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
    /// 组件销毁时释放单例引用。
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

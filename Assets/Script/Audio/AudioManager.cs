using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频管理类
/// - 管理 Addressables 音频加载、缓存、音源播放、音量设置与句柄释放。
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    #region 音源配置与加载缓存

    /// <summary>是否在切换场景时保留当前单例对象。</summary>
    protected override bool PersistAcrossScenes => true;

    [Tooltip("用于播放短音效的 2D AudioSource 列表。请确保列表中的元素不重复。")]
    [Header("Audio Source Pools")]
    [SerializeField] private List<AudioSource> audioSourcePool;

    [Tooltip("用于播放空间音效的 3D AudioSource 列表。请确保列表中的元素不重复。")]
    [SerializeField] private List<AudioSource> audioSources3DPool;

    /// <summary>统一提供音频加载、Label预加载和句柄管理的Addressables管理器。</summary>
    private AddressableManager addressableManager;

    /// <summary>已登记的 2D 音源与音频资源地址映射。</summary>
    private readonly Dictionary<AudioSource, string> playingSourceAddresses = new();

    /// <summary>已登记的空间音源与音频资源地址映射。</summary>
    private readonly Dictionary<AudioSource, string> playing3DSourceAddresses = new();

    [Tooltip("用于播放背景音乐的 AudioSource。该 Source 会随 AudioManager 跨场景保留。")]
    [Header("Background Music")]
    [SerializeField] private AudioSource bgmAudioSource;

    #endregion

    #region 用户音量读取

    /// <summary>当前音乐音量  (默认值)。</summary>
    private float _currentBgmVolume = 0.5f;

    /// <summary>当前音效音量  (默认值)。</summary>
    private float _currentSfxVolume = 0.5f;

    /// <summary>
    /// 读取当前背景音乐的用户音量设置。
    /// </summary>
    /// <returns>零到一之间的用户音量，不包含临时淡入淡出倍率。</returns>
    public float GetCurrentBgmVolume() => _currentBgmVolume;

    /// <summary>
    /// 读取当前音效的用户音量设置。
    /// </summary>
    /// <returns>零到一之间的音效音量。</returns>
    public float GetCurrentSfxVolume() => _currentSfxVolume;

    #endregion

    #region 生命周期

    /// <summary>
    /// 注册跨场景音频单例，并让重复实例退出初始化。
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        // 音频管理器是项目现有的全局入口，确保Addressables管理器随其一并建立。
        addressableManager = AddressableManager.GetOrCreate(gameObject);
    }

    #endregion

    #region 音频加载

    /// <summary>
    /// 通过统一Addressables管理器请求音频；缓存命中时同步返回，未命中时异步加载。
    /// </summary>
    /// <param name="address">非空的 Addressables 音频资源地址。</param>
    /// <param name="onLoaded">资源可用时执行的回调；缓存命中时同步执行，加载失败时不调用。</param>
    private void RequestClip(string address, Action<AudioClip> onLoaded)
    {
        if (addressableManager == null)
            addressableManager = AddressableManager.GetOrCreate(gameObject);

        addressableManager.LoadAsset(address, onLoaded);
    }

    #endregion

    #region 二维音效播放

    /// <summary>
    /// 请求加载并使用空闲音源播放 2D 音效。
    /// </summary>
    /// <param name="address">Addressables 音频资源地址。</param>
    public void PlaySound(string address)
    {
        RequestClip(address, clip => PlayClip(clip, address));
    }

    /// <summary>
    /// 停止当前已登记的指定地址的全部 2D 音效；不会取消尚未完成的加载。
    /// </summary>
    /// <param name="address">要停止播放的音频资源地址。</param>
    public void StopSound(string address)
    {
        StopSourcesForAddress(playingSourceAddresses, address);
    }

    /// <summary>
    /// 从 2D 音源池取得空闲源并播放片段，记录音源与资源地址的对应关系。
    /// </summary>
    /// <param name="clip">已加载的待播放音频片段。</param>
    /// <param name="address">片段所属资源地址，用于后续停止播放。</param>
    private void PlayClip(AudioClip clip, string address)
    {
        AudioSource freeSource = GetFreeAudioSource();      //获取空闲音源池
        if (freeSource != null)
        {
            //播放clip
            freeSource.volume = _currentSfxVolume;      //更新音量
            freeSource.clip = clip;
            freeSource.Play();
            playingSourceAddresses[freeSource] = address;
        }
        else
        {
            Debug.LogWarning("所有音源都在使用中，无法播放新音效！");
        }
    }

    /// <summary>
    /// 清理已结束的 2D 播放记录，并查找未播放的有效音源。
    /// </summary>
    /// <returns>首个空闲音源；没有可用音源时返回 null。</returns>
    private AudioSource GetFreeAudioSource()
    {
        CleanupFinishedSources(playingSourceAddresses);

        foreach (AudioSource source in audioSourcePool)
        {
            if (source != null && !source.isPlaying)
                return source;
        }
        return null; // 都在播放
    }

    #endregion

    #region 空间音效与播放记录

    /// <summary>
    /// 请求加载并在指定世界位置播放空间音效。
    /// </summary>
    /// <param name="address">Addressables 音频资源地址。</param>
    /// <param name="position">音源播放时采用的世界坐标。</param>
    public void PlaySound3D(string address, Vector3 position)
    {
        RequestClip(address, clip => PlayClip3D(clip, address, position));
    }

    /// <summary>
    /// 停止当前已登记的指定地址的全部 3D 音效；不会取消尚未完成的加载。
    /// </summary>
    /// <param name="address">要停止播放的音频资源地址。</param>
    public void StopSound3D(string address)
    {
        StopSourcesForAddress(playing3DSourceAddresses, address);
    }

    /// <summary>
    /// 从 3D 音源池取得空闲源，设置位置和音量后播放并登记资源地址。
    /// </summary>
    /// <param name="clip">已加载的待播放音频片段。</param>
    /// <param name="address">片段所属资源地址，用于后续停止播放。</param>
    /// <param name="position">音源应移动到的世界坐标。</param>
    private void PlayClip3D(AudioClip clip, string address, Vector3 position)
    {
        AudioSource source = GetFreeAudioSource3D();
        if (source != null)
        {
            source.transform.position = position;  // 设置音效播放的位置
            source.volume = _currentSfxVolume;     //更新音量
            source.clip = clip;
            source.spatialBlend = 1f;              // 确保是3D音效
            source.Play();

            playing3DSourceAddresses[source] = address;
        }
        else
        {
            Debug.LogWarning("所有3D音源都在使用中，无法播放新音效！");
        }
    }

    /// <summary>
    /// 清理已结束的空间音效记录，并查找未播放的有效音源。
    /// </summary>
    /// <returns>首个空闲空间音源；没有可用音源时返回 null。</returns>
    private AudioSource GetFreeAudioSource3D()
    {
        CleanupFinishedSources(playing3DSourceAddresses);

        foreach (AudioSource source in audioSources3DPool)
        {
            if (source != null && !source.isPlaying)
                return source;
        }
        return null;
    }

    /// <summary>
    /// 停止指定地址对应的已登记音源，清空其片段并移除播放记录。
    /// </summary>
    /// <param name="sourceAddresses">音源到资源地址的播放记录表，方法会移除匹配条目。</param>
    /// <param name="address">要停止的资源地址。</param>
    private static void StopSourcesForAddress(Dictionary<AudioSource, string> sourceAddresses, string address)
    {
        List<AudioSource> sourcesToRemove = new();

        foreach (KeyValuePair<AudioSource, string> pair in sourceAddresses)
        {
            if (pair.Value != address)
                continue;

            if (pair.Key != null)
            {
                pair.Key.Stop();
                pair.Key.clip = null;
            }

            sourcesToRemove.Add(pair.Key);
        }

        foreach (AudioSource source in sourcesToRemove)
            sourceAddresses.Remove(source);
    }

    /// <summary>
    /// 从播放记录表移除已销毁或不再播放的音源，不释放音频资源。
    /// </summary>
    /// <param name="sourceAddresses">需要清理的音源到资源地址映射。</param>
    private static void CleanupFinishedSources(Dictionary<AudioSource, string> sourceAddresses)
    {
        List<AudioSource> sourcesToRemove = new();

        foreach (KeyValuePair<AudioSource, string> pair in sourceAddresses)
        {
            if (pair.Key == null || !pair.Key.isPlaying)
                sourcesToRemove.Add(pair.Key);
        }

        foreach (AudioSource source in sourcesToRemove)
            sourceAddresses.Remove(source);
    }

    #endregion

    #region 背景音乐与音量设置

    /// <summary>
    /// 请求加载背景音乐，在回调中设置用户音量并播放。
    /// </summary>
    /// <param name="address">Addressables 背景音乐资源地址。</param>
    public void PlayBGM(string address)
    {
        RequestClip(address, clip =>
        {
            if (bgmAudioSource == null)
                return;

            bgmAudioSource.volume = _currentBgmVolume;
            bgmAudioSource.clip = clip;
            bgmAudioSource.Play();
        });
    }

    /// <summary>
    /// 停止并清空当前背景音乐。
    /// </summary>
    public void ClearBGM()
    {
        if (bgmAudioSource == null)
            return;

        bgmAudioSource.Stop();
        bgmAudioSource.clip = null;
    }

    /// <summary>
    /// 将临时倍率乘以用户背景音乐音量后应用到音源，不修改用户设置。
    /// </summary>
    /// <param name="volumeMultiplier">临时音量倍率，使用前限制在零到一。</param>
    public void AdjustBGMVolume(float volumeMultiplier)
    {
        float adjustedVolume = Mathf.Clamp01(volumeMultiplier) * _currentBgmVolume;
        bgmAudioSource.volume = adjustedVolume;
    }

    /// <summary>
    /// 暂停当前背景音乐，或从暂停位置继续播放。
    /// </summary>
    /// <param name="isPause">为 true 时暂停，为 false 时继续。</param>
    public void PauseOrContinueBGM(bool isPause)
    {
        if (isPause)
            bgmAudioSource.Pause();
        else
            bgmAudioSource.UnPause();
    }

    /// <summary>
    /// 保存用户背景音乐音量并立即应用到背景音乐源。
    /// </summary>
    /// <param name="volume">目标音量，使用前限制在零到一。</param>
    public void SetBgmVolume(float volume)
    {
        _currentBgmVolume = Mathf.Clamp01(volume);
        bgmAudioSource.volume = _currentBgmVolume;
    }

    /// <summary>
    /// 保存用户音效音量，并同步到所有有效的 2D 和 3D 音源。
    /// </summary>
    /// <param name="volume">目标音量，使用前限制在零到一。</param>
    public void SetSfxVolume(float volume)
    {
        _currentSfxVolume = Mathf.Clamp01(volume);

        //立刻更新池里所有空闲或正在播放的 2D 源
        foreach (var src in audioSourcePool)
            if (src != null)
                src.volume = _currentSfxVolume;

        //立刻更新所有 3D 源
        foreach (var src in audioSources3DPool)
            if (src != null)
                src.volume = _currentSfxVolume;
    }

    #endregion
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioManager : MonoBehaviour
{
    #region Inspector Configuration

    public static AudioManager Instance { get; private set; }

    [Header("Audio Source Pools")]
    [Tooltip("用于播放短音效的 2D AudioSource 列表。请确保列表中的元素不重复。")]
    [SerializeField] private List<AudioSource> audioSourcePool;

    [Tooltip("用于播放空间音效的 3D AudioSource 列表。请确保列表中的元素不重复。")]
    [SerializeField] private List<AudioSource> audioSources3DPool;

    private readonly Dictionary<string, AudioClip> loadedClips = new();
    private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> loadedClipHandles = new();
    private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> loadingClipHandles = new();
    private readonly Dictionary<string, List<Action<AudioClip>>> pendingClipLoads = new();

    private readonly Dictionary<AudioSource, string> playingSourceAddresses = new();
    private readonly Dictionary<AudioSource, string> playing3DSourceAddresses = new();

    [Header("Background Music")]
    [Tooltip("用于播放背景音乐的 AudioSource。该 Source 会随 AudioManager 跨场景保留。")]
    [SerializeField] private AudioSource bgmAudioSource;

    #endregion

    #region Volume State


    private float _currentBgmVolume = 0.5f;   //当前音乐音量  (默认值)
    private float _currentSfxVolume = 0.5f;   //当前音效音量  (默认值)

    /// <summary>获取当前背景音乐音量。</summary>
    public float GetCurrentBgmVolume() => _currentBgmVolume;

    /// <summary>获取当前音效音量。</summary>
    public float GetCurrentSfxVolume() => _currentSfxVolume;

    #endregion

    #region Lifecycle

    private void Awake()
    {
        // 如果已有实例且不是当前这个，就销毁它（避免重复）
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;    //单例化
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        foreach (AsyncOperationHandle<AudioClip> handle in loadedClipHandles.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        foreach (AsyncOperationHandle<AudioClip> handle in loadingClipHandles.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
    }

    #endregion

    #region Clip Loading

    /// <summary>
    /// 请求一个音频片段；相同地址在加载期间会合并请求，加载完成后通知所有调用方。
    /// </summary>
    private void RequestClip(string address, Action<AudioClip> onLoaded)
    {
        if (loadedClips.TryGetValue(address, out AudioClip cachedClip))
        {
            onLoaded(cachedClip);
            return;
        }

        if (pendingClipLoads.TryGetValue(address, out List<Action<AudioClip>> callbacks))
        {
            callbacks.Add(onLoaded);
            return;
        }

        pendingClipLoads[address] = new List<Action<AudioClip>> { onLoaded };
        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(address);
        loadingClipHandles[address] = handle;
        handle.Completed += completedHandle =>
        {
            loadingClipHandles.Remove(address);

            if (!pendingClipLoads.TryGetValue(address, out List<Action<AudioClip>> pendingCallbacks))
                return;

            pendingClipLoads.Remove(address);

            if (completedHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"音频加载失败 [{address}]: {completedHandle.OperationException}");
                return;
            }

            AudioClip clip = completedHandle.Result;
            loadedClips[address] = clip;
            loadedClipHandles[address] = completedHandle;

            foreach (Action<AudioClip> callback in pendingCallbacks)
                callback(clip);
        };
    }

    #endregion

    #region One-Shot Audio

    /// <summary>异步加载并播放一个 2D 短音效。</summary>
    public void PlaySound(string address)
    {
        RequestClip(address, clip => PlayClip(clip, address));
    }

    /// <summary>停止指定地址对应的全部 2D 短音效。</summary>
    public void StopSound(string address)
    {
        StopSourcesForAddress(playingSourceAddresses, address);
    }

    /// <summary>从 2D 音效池中取出一个空闲 Source 播放片段。</summary>
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

    /// <summary>清理已自然播放结束的 Source，并返回一个可用 Source。</summary>
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

    #region Spatial Audio

    /// <summary>异步加载并播放一个 3D 短音效。</summary>
    public void PlaySound3D(string address, Vector3 position)
    {
        RequestClip(address, clip => PlayClip3D(clip, address, position));
    }

    /// <summary>停止指定地址对应的全部 3D 短音效。</summary>
    public void StopSound3D(string address)
    {
        StopSourcesForAddress(playing3DSourceAddresses, address);
    }

    /// <summary>从 3D 音效池中取出一个空闲 Source 播放片段。</summary>
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

    /// <summary>清理已自然播放结束的 3D Source，并返回一个可用 Source。</summary>
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

    /// <summary>停止指定地址对应的 Source，并移除播放归属记录。</summary>
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

    #region Background Music

    /// <summary>异步加载并播放背景音乐。</summary>
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

    /// <summary>停止并清空当前背景音乐。</summary>
    public void ClearBGM()
    {
        if (bgmAudioSource == null)
            return;

        bgmAudioSource.Stop();
        bgmAudioSource.clip = null;
    }

    /// <summary>按当前背景音乐音量乘数调整播放音量。</summary>
    public void AdjustBGMVolume(float volumeMultiplier)
    {
        float adjustedVolume = Mathf.Clamp01(volumeMultiplier) * _currentBgmVolume;
        bgmAudioSource.volume = adjustedVolume;
    }

    /// <summary>暂停或恢复当前背景音乐。</summary>
    public void PauseOrContinueBGM(bool isPause)
    {
        if (isPause)
            bgmAudioSource.Pause();
        else
            bgmAudioSource.UnPause();
    }

    /// <summary>设置背景音乐音量。</summary>
    public void SetBgmVolume(float volume)
    {
        _currentBgmVolume = Mathf.Clamp01(volume);
        bgmAudioSource.volume = _currentBgmVolume;
    }

    /// <summary>设置音效音量，并立即同步到音效池中的 Source。</summary>
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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private List<AudioSource> audioSourcePool;      //一次性音源池
    [SerializeField] private List<AudioSource> audioSources3DPool;   //一次性3D音源池

    private readonly Dictionary<string, AudioClip> loadedClips = new();
    private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> loadedClipHandles = new();
    private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> loadingClipHandles = new();
    private readonly Dictionary<string, List<Action<AudioClip>>> pendingClipLoads = new();

    private readonly Dictionary<AudioSource, string> playingSourceAddresses = new();
    private readonly Dictionary<AudioSource, string> playing3DSourceAddresses = new();

    [SerializeField] private AudioSource bgmAudioSource;        //背景音乐音源


    private float _currentBgmVolume = 0.5f;   //当前音乐音量  (默认值)
    private float _currentSfxVolume = 0.5f;   //当前音效音量  (默认值)

    public float GetCurrentBgmVolume() => _currentBgmVolume;    //外部获取BGM音量方法
    public float GetCurrentSfxVolume() => _currentSfxVolume;    //外部获取音效音量方法

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

    //单次音效播放方法(外部调用)
    public void PlaySound(string address)
    {
        RequestClip(address, clip => PlayClip(clip, address));
    }

    //停止播放输入地址的所有音效
    public void StopSound(string address)
    {
        StopSourcesForAddress(playingSourceAddresses, address);
    }

    //播放音效并记录播放来源
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

    //获取单次音源池的空闲音源
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

    //播放3D音效方法
    public void PlaySound3D(string address, Vector3 position)
    {
        RequestClip(address, clip => PlayClip3D(clip, address, position));
    }

    //停止3D音效播放
    public void StopSound3D(string address)
    {
        StopSourcesForAddress(playing3DSourceAddresses, address);
    }

    //3D音效播放
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

    //获取空闲3D音源池
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

    //播放背景音乐
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

    //清空背景音乐剪辑
    public void ClearBGM()
    {
        if (bgmAudioSource == null)
            return;

        bgmAudioSource.Stop();
        bgmAudioSource.clip = null;
    }

    // 调整背景音乐的音量(基于currentBgmVolume)
    public void AdjustBGMVolume(float volumeMultiplier)
    {
        float adjustedVolume = Mathf.Clamp01(volumeMultiplier) * _currentBgmVolume;
        bgmAudioSource.volume = adjustedVolume;
    }

    public void PauseOrContinueBGM(bool isPause)
    {
        if (isPause)
            bgmAudioSource.Pause();
        else
            bgmAudioSource.UnPause();
    }

    //设置背景音乐音量
    public void SetBgmVolume(float volume)
    {
        _currentBgmVolume = Mathf.Clamp01(volume);
        bgmAudioSource.volume = _currentBgmVolume;
    }

    //设置音效音量
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


}

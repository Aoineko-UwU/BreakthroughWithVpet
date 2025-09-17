using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private List<AudioSource> audioSourcePool;      //一次性音源池
    [SerializeField] private List<AudioSource> audioSources3DPool;   //一次性3D音源池

    private Dictionary<string, AudioClip> loadedClips = new();                     // 缓存已加载的 AudioClip

    private Dictionary<string, List<AudioSource>> playingSourcesByAddress = new();   //记录每个2D音效的播放音源
    private Dictionary<string, List<AudioSource>> playing3DSourcesByAddress = new(); //记录每个3D音效的播放音源

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
        DontDestroyOnLoad(this);
    }

    //单次音效播放方法(外部调用)
    public void PlaySound(string address)
    {
        //若该地址片段已缓存过
        if (loadedClips.TryGetValue(address, out AudioClip cachedClip))
        {
            PlayClip(cachedClip, address);      //片段播放
        }
        //否则进行异步片段加载
        else
        {
            Addressables.LoadAssetAsync<AudioClip>(address).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AudioClip clip = handle.Result;     //获取加载结果(AudioClip)
                    loadedClips[address] = clip;        //将地址加入缓存字典
                    PlayClip(clip, address);            //播放
                }
                else
                {
                    Debug.LogError($"音效加载失败: {handle.OperationException}");
                }
            };
        }
    }

    //停止播放输入地址的所有音效
    public void StopSound(string address)
    {
        //若发现输入的地址有与播放字典中对应的地址字段
        if (playingSourcesByAddress.TryGetValue(address, out List<AudioSource> sources))
        {
            //遍历音源池
            foreach (var source in sources)
            {
                //暂停其播放
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                    source.clip = null;
                }
            }

            playingSourcesByAddress[address].Clear(); // 清空引用
        }
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
            //若该片段地址不存在于“播放中字典”内，则记录
            if (!playingSourcesByAddress.ContainsKey(address))
                playingSourcesByAddress[address] = new List<AudioSource>();

            playingSourcesByAddress[address].Add(freeSource);
        }
        else
        {
            Debug.LogWarning("所有音源都在使用中，无法播放新音效！");
        }
    }

    //获取单次音源池的空闲音源
    private AudioSource GetFreeAudioSource()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (!source.isPlaying && source != null)
                return source;
        }
        return null; // 都在播放
    }

    //播放3D音效方法
    public void PlaySound3D(string address, Vector3 position)
    {
        //如果缓存中有音效，则使用缓存音效
        if (loadedClips.TryGetValue(address, out AudioClip cachedClip))
        {
            PlayClip3D(cachedClip, address, position);
        }

        //否则获取音效播放并存入缓存字典
        else
        {
            Addressables.LoadAssetAsync<AudioClip>(address).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AudioClip clip = handle.Result;
                    loadedClips[address] = clip;
                    PlayClip3D(clip, address, position);
                }
                else
                {
                    Debug.LogError($"3D音效加载失败: {handle.OperationException}");
                }
            };
        }
    }

    //停止3D音效播放
    public void StopSound3D(string address)
    {
        if (playing3DSourcesByAddress.TryGetValue(address, out List<AudioSource> sources))
        {
            foreach (var source in sources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                    source.clip = null;
                }
            }
            playing3DSourcesByAddress[address].Clear();
        }
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

            if (!playing3DSourcesByAddress.ContainsKey(address))
                playing3DSourcesByAddress[address] = new List<AudioSource>();

            playing3DSourcesByAddress[address].Add(source);
        }
        else
        {
            Debug.LogWarning("所有3D音源都在使用中，无法播放新音效！");
        }
    }

    //获取空闲3D音源池
    private AudioSource GetFreeAudioSource3D()
    {
        foreach (AudioSource source in audioSources3DPool)
        {
            if (!source.isPlaying && source != null)
                return source;
            if (source == null)
                Debug.Log($"音源" + source + "被销毁.");
        }
        return null;
    }

    //播放背景音乐
    public void PlayBGM(string address)
    {
        // 使用缓存的音效或异步加载
        if (loadedClips.TryGetValue(address, out AudioClip cachedClip))
        {
            bgmAudioSource.clip = cachedClip;
            bgmAudioSource.Play();
        }
        else
        {
            Addressables.LoadAssetAsync<AudioClip>(address).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AudioClip clip = handle.Result;
                    loadedClips[address] = clip;
                    bgmAudioSource.volume = _currentBgmVolume;  //更新当前音量
                    bgmAudioSource.clip = clip;
                    bgmAudioSource.Play();
                }
                else
                {
                    Debug.LogError($"背景音乐加载失败: {handle.OperationException}");
                }
            };
        }
    }

    //清空背景音乐剪辑
    public void ClearBGM()
    {
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

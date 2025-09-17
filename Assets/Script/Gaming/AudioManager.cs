using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private List<AudioSource> audioSourcePool;      //һ������Դ��
    [SerializeField] private List<AudioSource> audioSources3DPool;   //һ����3D��Դ��

    private Dictionary<string, AudioClip> loadedClips = new();                     // �����Ѽ��ص� AudioClip

    private Dictionary<string, List<AudioSource>> playingSourcesByAddress = new();   //��¼ÿ��2D��Ч�Ĳ�����Դ
    private Dictionary<string, List<AudioSource>> playing3DSourcesByAddress = new(); //��¼ÿ��3D��Ч�Ĳ�����Դ

    [SerializeField] private AudioSource bgmAudioSource;        //����������Դ


    private float _currentBgmVolume = 0.5f;   //��ǰ��������  (Ĭ��ֵ)
    private float _currentSfxVolume = 0.5f;   //��ǰ��Ч����  (Ĭ��ֵ)

    public float GetCurrentBgmVolume() => _currentBgmVolume;    //�ⲿ��ȡBGM��������
    public float GetCurrentSfxVolume() => _currentSfxVolume;    //�ⲿ��ȡ��Ч��������

    private void Awake()
    {
        // �������ʵ���Ҳ��ǵ�ǰ��������������������ظ���
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;    //������
        DontDestroyOnLoad(this);
    }

    //������Ч���ŷ���(�ⲿ����)
    public void PlaySound(string address)
    {
        //���õ�ַƬ���ѻ����
        if (loadedClips.TryGetValue(address, out AudioClip cachedClip))
        {
            PlayClip(cachedClip, address);      //Ƭ�β���
        }
        //��������첽Ƭ�μ���
        else
        {
            Addressables.LoadAssetAsync<AudioClip>(address).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    AudioClip clip = handle.Result;     //��ȡ���ؽ��(AudioClip)
                    loadedClips[address] = clip;        //����ַ���뻺���ֵ�
                    PlayClip(clip, address);            //����
                }
                else
                {
                    Debug.LogError($"��Ч����ʧ��: {handle.OperationException}");
                }
            };
        }
    }

    //ֹͣ���������ַ��������Ч
    public void StopSound(string address)
    {
        //����������ĵ�ַ���벥���ֵ��ж�Ӧ�ĵ�ַ�ֶ�
        if (playingSourcesByAddress.TryGetValue(address, out List<AudioSource> sources))
        {
            //������Դ��
            foreach (var source in sources)
            {
                //��ͣ�䲥��
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                    source.clip = null;
                }
            }

            playingSourcesByAddress[address].Clear(); // �������
        }
    }

    //������Ч����¼������Դ
    private void PlayClip(AudioClip clip, string address)
    {
        AudioSource freeSource = GetFreeAudioSource();      //��ȡ������Դ��
        if (freeSource != null)
        {
            //����clip
            freeSource.volume = _currentSfxVolume;      //��������
            freeSource.clip = clip;
            freeSource.Play();
            //����Ƭ�ε�ַ�������ڡ��������ֵ䡱�ڣ����¼
            if (!playingSourcesByAddress.ContainsKey(address))
                playingSourcesByAddress[address] = new List<AudioSource>();

            playingSourcesByAddress[address].Add(freeSource);
        }
        else
        {
            Debug.LogWarning("������Դ����ʹ���У��޷���������Ч��");
        }
    }

    //��ȡ������Դ�صĿ�����Դ
    private AudioSource GetFreeAudioSource()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (!source.isPlaying && source != null)
                return source;
        }
        return null; // ���ڲ���
    }

    //����3D��Ч����
    public void PlaySound3D(string address, Vector3 position)
    {
        //�������������Ч����ʹ�û�����Ч
        if (loadedClips.TryGetValue(address, out AudioClip cachedClip))
        {
            PlayClip3D(cachedClip, address, position);
        }

        //�����ȡ��Ч���Ų����뻺���ֵ�
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
                    Debug.LogError($"3D��Ч����ʧ��: {handle.OperationException}");
                }
            };
        }
    }

    //ֹͣ3D��Ч����
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

    //3D��Ч����
    private void PlayClip3D(AudioClip clip, string address, Vector3 position)
    {
        AudioSource source = GetFreeAudioSource3D();
        if (source != null)
        {
            source.transform.position = position;  // ������Ч���ŵ�λ��
            source.volume = _currentSfxVolume;     //��������
            source.clip = clip;
            source.spatialBlend = 1f;              // ȷ����3D��Ч
            source.Play();

            if (!playing3DSourcesByAddress.ContainsKey(address))
                playing3DSourcesByAddress[address] = new List<AudioSource>();

            playing3DSourcesByAddress[address].Add(source);
        }
        else
        {
            Debug.LogWarning("����3D��Դ����ʹ���У��޷���������Ч��");
        }
    }

    //��ȡ����3D��Դ��
    private AudioSource GetFreeAudioSource3D()
    {
        foreach (AudioSource source in audioSources3DPool)
        {
            if (!source.isPlaying && source != null)
                return source;
            if (source == null)
                Debug.Log($"��Դ" + source + "������.");
        }
        return null;
    }

    //���ű�������
    public void PlayBGM(string address)
    {
        // ʹ�û������Ч���첽����
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
                    bgmAudioSource.volume = _currentBgmVolume;  //���µ�ǰ����
                    bgmAudioSource.clip = clip;
                    bgmAudioSource.Play();
                }
                else
                {
                    Debug.LogError($"�������ּ���ʧ��: {handle.OperationException}");
                }
            };
        }
    }

    //��ձ������ּ���
    public void ClearBGM()
    {
        bgmAudioSource.clip = null;
    }

    // �����������ֵ�����(����currentBgmVolume)
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

    //���ñ�����������
    public void SetBgmVolume(float volume)
    {
        _currentBgmVolume = Mathf.Clamp01(volume);
        bgmAudioSource.volume = _currentBgmVolume;
    }

    //������Ч����
    public void SetSfxVolume(float volume)
    {
        _currentSfxVolume = Mathf.Clamp01(volume);

        //���̸��³������п��л����ڲ��ŵ� 2D Դ
        foreach (var src in audioSourcePool)
            if (src != null)
                src.volume = _currentSfxVolume;

        //���̸������� 3D Դ
        foreach (var src in audioSources3DPool)
            if (src != null)
                src.volume = _currentSfxVolume;
    }


}

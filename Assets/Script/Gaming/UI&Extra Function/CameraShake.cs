using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;    //���������
    private CinemachineBasicMultiChannelPerlin noise;  //�����������չ���
    public static CameraShake Instance;                //����

    private void Awake()
    {
        Instance = this;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private float shakeFrequency = 0.2f;    //Ƶ��
    private float shakeAmplitude = 5;       //���
    private float initShakeFrequencyGain;   //Ĭ��Ƶ��
    private float initShakeAmplitude;       //Ĭ�����

    private void Start()
    {
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        initShakeFrequencyGain = noise.m_FrequencyGain;
        initShakeAmplitude = noise.m_AmplitudeGain;
    }

    //��Ļ�ζ�(�ⲿ����)
    public void ShakeScreen()
    {
        if(shakeScreenCoroutine != null)
        {
            StopCoroutine(shakeScreenCoroutine);
            shakeScreenCoroutine = StartCoroutine(Shake());
        }
        else
            shakeScreenCoroutine = StartCoroutine(Shake());
    }

    private Coroutine shakeScreenCoroutine;

    IEnumerator Shake()
    {
        noise.m_AmplitudeGain = shakeAmplitude;     //�������

        //�𽥼�ǿƵ��
        while(noise.m_FrequencyGain < shakeFrequency)
        {
            noise.m_FrequencyGain += 0.01f;
            yield return null;
        }

        //�𽥼���Ƶ��
        while(noise.m_FrequencyGain > initShakeFrequencyGain)
        {
            noise.m_FrequencyGain -= 0.002f;
            yield return null;
        }

        //�ָ�Ĭ��
        noise.m_FrequencyGain = initShakeFrequencyGain;
        noise.m_AmplitudeGain = initShakeAmplitude;

        shakeScreenCoroutine = null;    //����Э��
    }
}

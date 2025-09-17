using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;    //虚拟摄像机
    private CinemachineBasicMultiChannelPerlin noise;  //虚拟摄像机拓展组件
    public static CameraShake Instance;                //单例

    private void Awake()
    {
        Instance = this;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private float shakeFrequency = 0.2f;    //频率
    private float shakeAmplitude = 5;       //振幅
    private float initShakeFrequencyGain;   //默认频率
    private float initShakeAmplitude;       //默认振幅

    private void Start()
    {
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        initShakeFrequencyGain = noise.m_FrequencyGain;
        initShakeAmplitude = noise.m_AmplitudeGain;
    }

    //屏幕晃动(外部调用)
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
        noise.m_AmplitudeGain = shakeAmplitude;     //调整振幅

        //逐渐加强频率
        while(noise.m_FrequencyGain < shakeFrequency)
        {
            noise.m_FrequencyGain += 0.01f;
            yield return null;
        }

        //逐渐减弱频率
        while(noise.m_FrequencyGain > initShakeFrequencyGain)
        {
            noise.m_FrequencyGain -= 0.002f;
            yield return null;
        }

        //恢复默认
        noise.m_FrequencyGain = initShakeFrequencyGain;
        noise.m_AmplitudeGain = initShakeAmplitude;

        shakeScreenCoroutine = null;    //清理协程
    }
}

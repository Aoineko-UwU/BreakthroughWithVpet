using System.Collections;
using UnityEngine;
using Cinemachine;

/// <summary>
/// 镜头震动类
/// - 通过 Cinemachine 噪声参数产生可重复触发的镜头震动。
/// </summary>
public class CameraShake : Singleton<CameraShake>
{
    #region 配置与运行状态

    /// <summary>虚拟摄像机。</summary>
    private CinemachineVirtualCamera virtualCamera;

    /// <summary>虚拟摄像机拓展组件。</summary>
    private CinemachineBasicMultiChannelPerlin noise;

    #endregion

    #region 初始化与震动流程

    /// <summary>
    /// 注册单例；仅有效实例缓存 Cinemachine 虚拟镜头。
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    /// <summary>频率。</summary>
    private float shakeFrequency = 0.2f;

    /// <summary>振幅。</summary>
    private float shakeAmplitude = 5;

    /// <summary>默认频率。</summary>
    private float initShakeFrequencyGain;

    /// <summary>默认振幅。</summary>
    private float initShakeAmplitude;

    /// <summary>
    /// 取得噪声组件，并记录初始频率和振幅以便震动后恢复。
    /// </summary>
    private void Start()
    {
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        initShakeFrequencyGain = noise.m_FrequencyGain;
        initShakeAmplitude = noise.m_AmplitudeGain;
    }

    /// <summary>
    /// 触发一次屏幕震动；重复触发会重启当前震动协程。
    /// </summary>
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

    /// <summary>当前震动协程引用，重复触发时用于中止旧流程。</summary>
    private Coroutine shakeScreenCoroutine;

    /// <summary>
    /// 设置震动振幅，逐帧调整频率，完成后恢复初始噪声参数并清空协程引用。
    /// </summary>
    /// <returns>控制震动频率变化的协程迭代器。</returns>
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

    #endregion
}

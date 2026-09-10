using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;
/// <summary>
/// 管理镜头缩放 Slider 以及开场、死亡和胜利阶段的镜头动画。
/// </summary>
public class CameraScaleBar : MonoBehaviour
{
    [Tooltip("绑定虚拟摄像机")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera; //绑定虚拟摄像机
    [Tooltip("Vpet摄像机跟踪点绑定")]
    [SerializeField] private Transform vpetCameraTrackPoint;         //Vpet摄像机跟踪点绑定
    private Slider cameraScaleSlider;
    private float _newScale;
    private float scaleLerp = 0.03f;    //缩放插值(越小越平滑)
    private float keyScaleSpeed = 6f;   //按下键盘时的缩放速度(f/s)

    private void Awake()
    {
        cameraScaleSlider = GetComponent<Slider>(); //获取滑动条组件
        //初始化值
        _newScale = cameraScaleSlider.value; virtualCamera.m_Lens.OrthographicSize = cameraScaleSlider.value;
    }
    private void Update()
    {
        float differenceValue = Mathf.Abs(virtualCamera.m_Lens.OrthographicSize - _newScale); //获取新旧缩放值差值
        //差值若大于某阈值则进行平滑过渡
        if (differenceValue > 0.05f)
        {
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, _newScale, scaleLerp);
        }

        KeyControl();
    }
    /// <summary>根据 Slider 当前值调整镜头正交尺寸。</summary>
    public void ChangeCameraScale()
    {
        if (!GameManager.Instance.isAllowPlayerControl) return;
        _newScale = (cameraScaleSlider.minValue + cameraScaleSlider.maxValue) - cameraScaleSlider.value; //滑条值越大，缩放值越小
    }

    private void KeyControl()
    {
        if (!GameManager.Instance.isAllowPlayerControl) return;
        //按下A缩小
        if (Input.GetKey(KeyCode.A))
        {
            cameraScaleSlider.value -= keyScaleSpeed * Time.deltaTime; ChangeCameraScale();
        }
        //按下D放大
        if (Input.GetKey(KeyCode.D))
        {
            cameraScaleSlider.value += keyScaleSpeed * Time.deltaTime; ChangeCameraScale();
        }
    }

    /// <summary>播放游戏开场的镜头缩放动画。</summary>
    public void StartCameraScale()
    {
        StartCoroutine(CameraScaleAnimationStart());
    }

    IEnumerator CameraScaleAnimationStart()
    {
        AudioManager.Instance.PlaySound("camera");
        scaleLerp = 0.01f;
        _newScale = 2f;
        yield return new WaitForSeconds(2f);
        scaleLerp = 0.03f; _newScale = 4f;
    }

    /// <summary>播放桌宠死亡阶段的镜头特写。</summary>
    public void DieCameraScale()
    {
        StartCoroutine(CameraScaleAnimationDie());
    }
    IEnumerator CameraScaleAnimationDie()
    {
        _newScale = 5f;
        yield return new WaitForSeconds(1f);
        vpetCameraTrackPoint.DOLocalMove(Vector3.zero, 1.5f);
        scaleLerp = 0.01f; _newScale = 2f;
    }

    /// <summary>播放桌宠胜利阶段的镜头特写。</summary>
    public void WinCameraScale()
    {
        scaleLerp = 0.01f;
        _newScale = 2f;
        vpetCameraTrackPoint.DOLocalMove(Vector3.zero, 1f);
    }
}

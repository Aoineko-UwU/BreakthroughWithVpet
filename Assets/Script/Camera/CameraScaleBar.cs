using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;

/// <summary>
/// 镜头缩放控制类
/// - 同步滑条与键盘缩放输入，并提供开场和结算阶段的镜头动画。
/// </summary>
public class CameraScaleBar : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("绑定虚拟摄像机。")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Tooltip("Vpet摄像机跟踪点绑定。")]
    [SerializeField] private Transform vpetCameraTrackPoint;

    /// <summary>控制镜头缩放目标的 UI 滑条。</summary>
    private Slider cameraScaleSlider;

    /// <summary>平滑缩放需要接近的目标正交尺寸。</summary>
    private float _newScale;

    /// <summary>缩放插值(越小越平滑)。</summary>
    private float scaleLerp = 0.03f;

    /// <summary>按下键盘时的缩放速度(f/s)。</summary>
    private float keyScaleSpeed = 6f;

    #endregion

    #region 初始化与玩家缩放

    /// <summary>
    /// 缓存缩放滑条，以其当前值初始化目标尺寸和虚拟镜头正交尺寸。
    /// </summary>
    private void Awake()
    {
        cameraScaleSlider = GetComponent<Slider>(); //获取滑动条组件
        //初始化值
        _newScale = cameraScaleSlider.value; virtualCamera.m_Lens.OrthographicSize = cameraScaleSlider.value;
    }

    /// <summary>
    /// 当尺寸差超过阈值时平滑调整镜头，并读取键盘缩放输入。
    /// </summary>
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

    /// <summary>
    /// 允许玩家操作时，将滑条值反向映射为目标镜头正交尺寸。
    /// </summary>
    public void ChangeCameraScale()
    {
        if (!GameManager.Instance.isAllowPlayerControl) return;
        _newScale = (cameraScaleSlider.minValue + cameraScaleSlider.maxValue) - cameraScaleSlider.value; //滑条值越大，缩放值越小
    }

    /// <summary>
    /// 允许玩家操作时响应 A、D 按键调整滑条，并更新目标镜头尺寸。
    /// </summary>
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

    #endregion

    #region 开场与结算镜头

    /// <summary>
    /// 播放游戏开场的镜头缩放动画。
    /// </summary>
    public void StartCameraScale()
    {
        StartCoroutine(CameraScaleAnimationStart());
    }

    /// <summary>
    /// 短暂降低插值速度并拉近镜头，等待后恢复常规缩放目标。
    /// </summary>
    /// <returns>控制开场镜头停留时长的协程迭代器。</returns>
    IEnumerator CameraScaleAnimationStart()
    {
        AudioManager.Instance.PlaySound("camera");
        scaleLerp = 0.01f;
        _newScale = 2f;
        yield return new WaitForSeconds(2f);
        scaleLerp = 0.03f; _newScale = 4f;
    }

    /// <summary>
    /// 播放桌宠死亡阶段的镜头特写。
    /// </summary>
    public void DieCameraScale()
    {
        StartCoroutine(CameraScaleAnimationDie());
    }

    /// <summary>
    /// 先扩大镜头尺寸，等待后将跟踪点移回原点并拉近镜头。
    /// </summary>
    /// <returns>控制死亡镜头阶段等待的协程迭代器。</returns>
    IEnumerator CameraScaleAnimationDie()
    {
        _newScale = 5f;
        yield return new WaitForSeconds(1f);
        vpetCameraTrackPoint.DOLocalMove(Vector3.zero, 1.5f);
        scaleLerp = 0.01f; _newScale = 2f;
    }

    /// <summary>
    /// 播放桌宠胜利阶段的镜头特写。
    /// </summary>
    public void WinCameraScale()
    {
        scaleLerp = 0.01f;
        _newScale = 2f;
        vpetCameraTrackPoint.DOLocalMove(Vector3.zero, 1f);
    }

    #endregion
}

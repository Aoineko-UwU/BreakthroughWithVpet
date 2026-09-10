using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 地图进度条类
/// - 按桌宠横坐标更新关卡进度，并标记已到达的重生点。
/// </summary>
public class MapProgressBar : Singleton<MapProgressBar>
{
    #region 配置与运行状态

    [Tooltip("用于交互或距离判断的桌宠对象。")]
    [SerializeField] private Transform vpet;

    [Tooltip("存档点1的Image。")]
    [SerializeField] private Image checkPoint01;

    [Tooltip("存档点2的Image。")]
    [SerializeField] private Image checkPoint02;

    /// <summary>滑动条。</summary>
    private Slider slider;

    /// <summary>开始位置。</summary>
    private float startPosX = -14f;

    /// <summary>结束位置。</summary>
    private float endPosX = 550f;

    #endregion

    #region 地图进度与到达标记

    /// <summary>
    /// 注册单例；仅有效实例缓存进度滑条。
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        slider = GetComponent<Slider>();
    }

    /// <summary>
    /// 更新当前地图进度。
    /// </summary>
    private void Update()
    {
        SetProgressValue();
    }

    /// <summary>
    /// 根据桌宠横坐标计算起终点之间的进度，将其限制在零到一并同步滑条。
    /// </summary>
    private void SetProgressValue()
    {
        if (vpet == null || slider == null)
            return;

        float currentX = vpet.position.x;       //获取桌宠X轴位置
        float progress = Mathf.InverseLerp(startPosX, endPosX, currentX); // 自动返回桌宠位置相对startPosX和endPosX的位置，并自动钳制为0~1

        slider.value = progress;    //赋予value
    }

    /// <summary>
    /// 将指定重生点的图标设为已到达颜色。
    /// </summary>
    /// <param name="index">重生点序号：1 或 2；其他值仅输出日志。</param>
    public void SetArrive(int index)
    {
        switch (index)
        {
            case 1:
                checkPoint01.color = new Color(1, 0.45f, 1, 1);
                break;

            case 2:
                checkPoint02.color = new Color(1, 0.45f, 1, 1);
                break;

            default:
                Debug.Log("序号未知");
                break;

        }
    }

    #endregion
}

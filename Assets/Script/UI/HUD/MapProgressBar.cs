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

    [Tooltip("按重生点序号排列的进度图标，数组第一个元素对应序号 1。")]
    [SerializeField] private Image[] checkPointIcons;

    /// <summary>滑动条。</summary>
    private Slider slider;

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

        MapConfig map = GameManager.Instance.CurrentMapConfig;
        float currentX = vpet.position.x;       //获取桌宠X轴位置
        float progress = Mathf.InverseLerp(map.progressStartX, map.progressEndX, currentX);

        slider.value = progress;    //赋予value
    }

    /// <summary>
    /// 将指定重生点的图标设为已到达颜色。
    /// </summary>
    /// <param name="index">从 1 开始的重生点序号；越界时仅输出日志。</param>
    public void SetArrive(int index)
    {
        if (checkPointIcons == null || index < 1 || index > checkPointIcons.Length || checkPointIcons[index - 1] == null)
        {
            Debug.Log("序号未知");
            return;
        }

        checkPointIcons[index - 1].color = new Color(1, 0.45f, 1, 1);
    }

    #endregion
}

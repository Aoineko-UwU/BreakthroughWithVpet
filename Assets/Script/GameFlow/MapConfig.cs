using UnityEngine;

/// <summary>
/// 地图配置类
/// - 保存关卡进度范围与关卡背景音乐等地图级数据。
/// </summary>
[CreateAssetMenu(fileName = "NewMapConfig", menuName = "Game/Map Config")]
public sealed class MapConfig : ScriptableObject
{
    #region 进度与音乐

    [Tooltip("进度条起点的世界坐标 X。")]
    public float progressStartX;

    [Tooltip("进度条终点的世界坐标 X，必须大于起点。")]
    public float progressEndX;

    [Tooltip("通过音频管理器播放的关卡背景音乐地址。")]
    public string bgmAddress;

    #endregion
}

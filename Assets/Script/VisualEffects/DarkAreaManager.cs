using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 暗区管理类
/// - 统计桌宠触发暗区的次数，并平滑调整配置的 Tilemap 颜色亮度。
/// </summary>
public class DarkAreaManager : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("需要随暗区状态调整亮度的 Tilemap 列表。")]
    [SerializeField] private List<Tilemap> tilemaps;

    /// <summary>最低亮度。</summary>
    private float darkestColor = 0.55f;

    /// <summary>变亮速度。</summary>
    private float lightSpeed = 0.2f;

    /// <summary>变暗速度。</summary>
    private float darkSpeed = 0.2f;

    /// <summary>当前已登记且尚未退出的桌宠触发次数，用于支持重叠暗区。</summary>
    private int vpetTriggerCount = 0;

    /// <summary>是否颜色需要改变。</summary>
    private bool colorNeedsUpdate = false;

    #endregion

    #region 区域计数与亮度渐变

    /// <summary>
    /// 增加桌宠进入计数，并请求重新计算暗区颜色。
    /// </summary>
    public void RegisterVpetEnter()
    {
        vpetTriggerCount++;
        colorNeedsUpdate = true;
    }

    /// <summary>
    /// 递减桌宠进入计数且最低为零，并请求重新计算暗区颜色。
    /// </summary>
    public void RegisterVpetExit()
    {
        vpetTriggerCount = Mathf.Max(0, vpetTriggerCount - 1);
        colorNeedsUpdate = true;
    }

    /// <summary>
    /// 存在颜色更新请求时渐变调整各 Tilemap；全部停止变化后关闭更新。
    /// </summary>
    private void Update()
    {
        if (!colorNeedsUpdate) return;      //颜色无需改变则不执行

        bool vpetInsideAny = vpetTriggerCount > 0;  //存在vpet时为真
        bool stillChanging = false;                 //颜色是否仍然在更变？

        foreach (Tilemap tile in tilemaps)
        {
            Color oldColor = tile.color;
            Color newColor = AdjustColor(oldColor, vpetInsideAny);
            tile.color = newColor;

            if (newColor != oldColor)
                stillChanging = true;
        }

        // 如果所有颜色都已到达目标值，停止后续更新
        colorNeedsUpdate = stillChanging;
    }

    /// <summary>
    /// 将 RGB 分量逐步靠近暗区或正常亮度，并保留原透明度。
    /// </summary>
    /// <param name="original">本帧调整前的颜色。</param>
    /// <param name="isDarkening">是否使用暗区目标亮度与变暗速度。</param>
    /// <returns>向目标亮度推进一个帧间隔后的颜色。</returns>
    private Color AdjustColor(Color original, bool isDarkening)
    {
        float speed = isDarkening ? darkSpeed : lightSpeed;
        float target = isDarkening ? darkestColor : 1f;

        float r = Mathf.MoveTowards(original.r, target, speed * Time.deltaTime);
        float g = Mathf.MoveTowards(original.g, target, speed * Time.deltaTime);
        float b = Mathf.MoveTowards(original.b, target, speed * Time.deltaTime);

        return new Color(r, g, b, original.a);
    }

    #endregion
}

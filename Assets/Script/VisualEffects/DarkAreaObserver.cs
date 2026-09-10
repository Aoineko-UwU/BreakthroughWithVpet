using UnityEngine;

/// <summary>
/// 暗区触发观察类
/// - 将桌宠进入或离开触发区域的事件转交暗区管理器。
/// </summary>
public class DarkAreaObserver : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("接收桌宠进出通知并调整 Tilemap 亮度的暗区管理器。")]
    [SerializeField] private DarkAreaManager controller;

    #endregion

    #region 暗区进出通知

    /// <summary>
    /// 桌宠进入触发区域时通知暗区管理器增加计数。
    /// </summary>
    /// <param name="other">进入暗区触发器的碰撞体。</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Vpet"))
            controller.RegisterVpetEnter();
    }

    /// <summary>
    /// 桌宠离开触发区域时通知暗区管理器减少计数。
    /// </summary>
    /// <param name="other">离开暗区触发器的碰撞体。</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Vpet"))
            controller.RegisterVpetExit();
    }

    #endregion
}

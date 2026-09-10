using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物品拾取触发类
/// - 将桌宠进入或持续停留在触发器中的事件转交场景物品。
/// </summary>
public class CanPickItem_Pick : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("接收拾取请求的关联场景物品。")]
    [SerializeField] private CanPickItem pickItem;

    #endregion

    #region 拾取触发

    /// <summary>
    /// 桌宠进入触发器时请求拾取关联的场景物品。
    /// </summary>
    /// <param name="other">进入触发区域的碰撞体。</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Vpet"))
            pickItem.ItemPickUpLogic();
    }

    /// <summary>
    /// 桌宠持续停留在触发器中时重试拾取，供物品栏释放空位后使用。
    /// </summary>
    /// <param name="other">持续位于触发区域内的碰撞体。</param>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Vpet"))
            pickItem.ItemPickUpLogic();
    }

    #endregion
}

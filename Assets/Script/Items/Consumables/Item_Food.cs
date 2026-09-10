using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 食物道具类
/// - 管理食物存在时间，并在接触可进食的桌宠时请求进食。
/// </summary>
public class Item_Food : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("当前场景物品对应的物品数据。")]
    [SerializeField] private ItemData itemData;

    [Tooltip("食物生成后允许存在的时间，单位为秒。")]
    [SerializeField] private float liftTime = 8f;

    #endregion

    #region 食物生命周期与进食交互

    /// <summary>
    /// 安排食物在配置的存在时间后自动销毁。
    /// </summary>
    private void Start()
    {
        Destroy(gameObject, liftTime);    //存在一定时间后销毁
    }

    /// <summary>
    /// 持续接触允许进食的桌宠时销毁自身，并传入食物数据请求进食。
    /// </summary>
    /// <param name="other">持续接触的碰撞信息。</param>
    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Vpet"))
        {
            VpetAction vpet = other.gameObject.GetComponent<VpetAction>();
            if (vpet.isAllowEat)
            {
                Destroy(gameObject);     //销毁自身
                vpet.VpetEat(itemData);  //调用玩家进食行为
            }
        }
    }

    /// <summary>
    /// 首次接触允许进食的桌宠时销毁自身，并传入食物数据请求进食。
    /// </summary>
    /// <param name="other">首次接触的碰撞信息。</param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Vpet"))
        {
            VpetAction vpet = other.gameObject.GetComponent<VpetAction>();
            if (vpet.isAllowEat)
            {
                Destroy(gameObject);     //销毁自身
                vpet.VpetEat(itemData);  //调用玩家进食行为
            }
        }
    }

    #endregion
}

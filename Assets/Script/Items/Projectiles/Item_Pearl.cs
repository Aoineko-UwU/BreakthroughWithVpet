using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 传送珍珠类
/// - 控制珍珠存在时间，并在命中非排除标签的碰撞体时传送桌宠及请求受伤。
/// </summary>
public class Item_Pearl : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>用于交互或距离判断的桌宠对象。</summary>
    private GameObject vpet;

    #endregion

    #region 珍珠生命周期与传送

    /// <summary>
    /// 缓存要传送的桌宠对象。
    /// </summary>
    private void Awake()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");    //获取桌宠的游戏对象
    }

    /// <summary>
    /// 安排珍珠在八秒后自动销毁。
    /// </summary>
    private void Start()
    {
        Destroy(gameObject, 8f);
    }

    /// <summary>
    /// 接触非 Item、Vpet、Ignore、Enemy 标签的碰撞体时，将桌宠传送到珍珠位置并请求伤害。
    /// </summary>
    /// <param name="other">珍珠当前重叠的碰撞体。</param>
    private void OnTriggerStay2D(Collider2D other)
    {
       if(!other.CompareTag("Item") && !other.CompareTag("Vpet") && !other.CompareTag("Ignore") && !other.CompareTag("Enemy"))
        {
            vpet.transform.position = transform.position;
            vpet.GetComponent<VpetHealthSystem>().VpetGethurt(3f, Vector2.zero);
            AudioManager.Instance.PlaySound("teleport");
            Destroy(gameObject);
        }
    }

    #endregion
}

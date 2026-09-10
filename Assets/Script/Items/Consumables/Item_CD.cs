using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 光盘道具类
/// - 管理光盘存在时间，在接触可交互的桌宠时切换跳舞状态并生成光效。
/// </summary>
public class Item_CD : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("桌宠跳舞时生成并附着在桌宠上的光效预制体。")]
    [SerializeField] private GameObject lightPrefab;

    /// <summary>当前接触的桌宠行为组件。</summary>
    private VpetAction vpet;

    #endregion

    #region 光盘生命周期与跳舞交互

    /// <summary>
    /// 安排光盘在五秒后自动销毁。
    /// </summary>
    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    /// <summary>
    /// 持续接触允许进食的桌宠时销毁光盘，切换跳舞状态并生成两份附着光效。
    /// </summary>
    /// <param name="other">持续接触的碰撞信息。</param>
    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Vpet"))
        {
            vpet = other.gameObject.GetComponent<VpetAction>();
            Transform vpetTransform = other.transform;

            if (vpet.isAllowEat)
            {
                Destroy(gameObject);    //销毁CD
                vpet.VpetStateSet(6);   //设置状态
                Instantiate(lightPrefab, vpetTransform.position, Quaternion.identity, vpetTransform);   //添加光效
                Instantiate(lightPrefab, vpetTransform.position, Quaternion.identity, vpetTransform);   //添加光效
            }
        }
    }

    /// <summary>
    /// 首次接触允许进食的桌宠时销毁光盘，切换跳舞状态并生成两份附着光效。
    /// </summary>
    /// <param name="other">首次接触的碰撞信息。</param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Vpet"))
        {
            vpet = other.gameObject.GetComponent<VpetAction>();
            Transform vpetTransform = other.transform;

            if (vpet.isAllowEat)
            {
                Destroy(gameObject);    //销毁CD
                vpet.VpetStateSet(6);   //设置状态
                Instantiate(lightPrefab, vpetTransform.position, Quaternion.identity, vpetTransform);   //添加光效
                Instantiate(lightPrefab, vpetTransform.position, Quaternion.identity, vpetTransform);   //添加光效
            }
        }
    }

    #endregion
}

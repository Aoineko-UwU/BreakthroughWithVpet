using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弹簧方块类
/// - 沿自身上方向弹起接触的物品、桌宠或敌人，并管理触发冷却与动画。
/// </summary>
public class Item_Block_Spring : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>当前对象的动画器。</summary>
    private Animator animator;

    #endregion

    #region 弹跳触发与冷却

    /// <summary>
    /// 缓存弹簧动画器。
    /// </summary>
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>弹簧是否已触发且正在等待冷却结束。</summary>
    private bool isActive = false;

    [Tooltip("弹簧力(冲击力)。")]
    [SerializeField]private float bounceForce = 13f;

    [Tooltip("触发后再次可用的冷却时长，单位为秒。")]
    [SerializeField] private float resetTime = 10f;

    /// <summary>
    /// 将触发冷却标记同步给动画器。
    /// </summary>
    private void Update()
    {
        animator.SetBool("isActive", isActive);
    }

    /// <summary>
    /// 冷却结束后弹起接触的物品、桌宠或敌人，播放音效并安排冷却重置。
    /// </summary>
    /// <param name="other">持续接触的碰撞信息，目标须带有刚体。</param>
    private void OnCollisionStay2D(Collision2D other)
    {
        if (isActive) return;

        if (other.collider.CompareTag("Item") || other.collider.CompareTag("Vpet") || other.collider.CompareTag("Enemy"))
        {
            isActive = true;
            other.rigidbody.AddForce(transform.up.normalized * bounceForce * other.rigidbody.mass, ForceMode2D.Impulse);
            AudioManager.Instance.PlaySound3D("spring_active", transform.position);
            Invoke("ResetSpring", resetTime);
        }
    }

    /// <summary>
    /// 清除已触发标记，使弹簧能够再次弹起接触对象。
    /// </summary>
    private void ResetSpring()
    {
        isActive = false;
    }

    #endregion
}

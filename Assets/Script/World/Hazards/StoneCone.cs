using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 石锥陷阱类
/// - 检测桌宠接近，控制石锥下落、触发伤害与破碎表现。
/// </summary>
public class StoneCone : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>当前对象的二维刚体组件。</summary>
    private Rigidbody2D rb;

    /// <summary>用于交互或距离判断的桌宠对象。</summary>
    private GameObject vpet;

    [Tooltip("破碎粒子。")]
    [SerializeField]private GameObject breakParticle;

    #endregion

    #region 初始化与状态更新

    /// <summary>
    /// 缓存桌宠对象与石锥刚体。
    /// </summary>
    private void Awake()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //获取桌宠的游戏对象
        rb = GetComponent<Rigidbody2D>();                 //获取刚体
    }

    /// <summary>
    /// 按当前难度初始化伤害数值。
    /// </summary>
    private void Start()
    {
        InitValueBasedDifficulty();     //初始化数值
    }

    /// <summary>
    /// 检查桌宠是否进入下落触发的水平范围。
    /// </summary>
    private void Update()
    {
        CheckVpetArrive();
    }

    #endregion

    #region 难度与下落触发

    /// <summary>
    /// 根据难度设置对桌宠和可破坏方块的伤害。
    /// </summary>
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配(对桌宠伤害|对建筑伤害)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                damageToVpet = 3f;
                damageToBlock = 6f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                damageToVpet = 4f;
                damageToBlock = 8f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                damageToVpet = 5f;
                damageToBlock = 10f;
                break;
        }
    }

    [Tooltip("是否已进入下落流程，避免重复触发。")]
    public bool isFalling = false;

    /// <summary>是否已启用石锥碰撞伤害与破碎检测。</summary>
    private bool isStartCheck = false;

    /// <summary>触发下落后延迟开启碰撞检测的时间，单位为秒。</summary>
    private float startCheckTime = 0.3f;

    /// <summary>命中桌宠时请求扣除的生命值。</summary>
    private float damageToVpet = 4f;

    /// <summary>命中可破坏方块时扣除的生命值。</summary>
    private float damageToBlock = 8f;

    /// <summary>
    /// 桌宠横向距离小于阈值时解除垂直移动约束，并延迟开启伤害检测。
    /// </summary>
    private void CheckVpetArrive()
    {
        if (isFalling) return;
        Vector2 vpetV2PosX = new Vector2(vpet.transform.position.x, transform.position.y);
        if (Vector2.Distance(vpetV2PosX, transform.position) < 2f)
        {
            isFalling = true;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;    //解除Y轴限制
            Invoke("StartCheck", startCheckTime);
        }
    }

    /// <summary>
    /// 解除垂直移动约束，开启碰撞伤害检测并安排超时销毁，以微小力唤醒刚体。
    /// </summary>
    public void StartCheck()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;    //解除Y轴限制
        isStartCheck = true;
        Destroy(gameObject, 5f);
        rb.AddForce(Vector2.up * 0.1f);     //激活刚体
    }

    #endregion

    #region 伤害与破碎

    /// <summary>
    /// 检测开启后接触地面或桌宠时请求相应伤害，播放破碎表现并销毁自身。
    /// </summary>
    /// <param name="other">进入石锥触发区域的碰撞体。</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isStartCheck) return;

        if(other.CompareTag("Ground") || other.CompareTag("Vpet"))
        {
            if (other.CompareTag("Vpet"))
            {
                other.GetComponent<VpetHealthSystem>().VpetGethurt(damageToVpet, (Vector2.left + Vector2.up) * 40f);
            }
            if (other.CompareTag("Ground") && other.GetComponent<Item_Block>() != null)
                other.GetComponent<Item_Block>().GetHurt(damageToBlock);

            AudioManager.Instance.PlaySound3D("stoneBreak", transform.position);    //播放音效
            Instantiate(breakParticle, transform.position, Quaternion.identity);    //生成破碎粒子
            Destroy(gameObject);    //销毁
        }
    }

    #endregion
}

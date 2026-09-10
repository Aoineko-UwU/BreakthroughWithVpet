using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蝙蝠敌人行为类
/// - 控制蝙蝠在水平区间内巡逻、转向、播放振翅音效和接触攻击。
/// </summary>
public class Enemy02_Bat : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>当前对象的二维刚体组件。</summary>
    private Rigidbody2D rb;

    [Tooltip("生命系统脚本。")]
    [SerializeField] private EnemyHealthSystem healthSystem;

    /// <summary>巡逻点1。</summary>
    private Vector3 pointA;

    /// <summary>巡逻点2。</summary>
    private Vector3 pointB;

    /// <summary>巡逻点设置范围。</summary>
    private float pointRange = 5f;

    /// <summary>唯一面向向量值。</summary>
    private float faceDir;

    /// <summary>巡逻速度参数；写入刚体速度前会乘以物理帧间隔。</summary>
    private float moveSpeed = 160f;

    /// <summary>包含左右方向符号的当前巡逻速度参数。</summary>
    private float currentSpeed;

    /// <summary>用于交互或距离判断的桌宠对象。</summary>
    private GameObject vpet;

    #endregion

    #region 生命周期

    /// <summary>
    /// 缓存刚体和桌宠对象引用。
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        vpet = GameObject.FindGameObjectWithTag("Vpet");  //获取桌宠的游戏对象
    }

    /// <summary>
    /// 记录初始移动参数、朝向与巡逻范围，再加载难度配置。
    /// </summary>
    private void Start()
    {
        currentSpeed = moveSpeed;
        faceDir = transform.localScale.x;
        pointA = new Vector3(transform.position.x - pointRange, transform.position.y); //设置PointA
        pointB = new Vector3(transform.position.x + pointRange, transform.position.y); //设置PointB

        InitValueBasedDifficulty();         //初始化难度相关数值

    }

    /// <summary>是否允许播放音频。</summary>
    private bool isAllowAudioPlay = true;

    /// <summary>
    /// 按桌宠距离更新音效开关，递减音效计时并处理巡逻转向。
    /// </summary>
    private void Update()
    {
        //与玩家距离超过20f后禁止播放音效
        isAllowAudioPlay = Vector2.Distance(vpet.transform.position, transform.position) > 20f ? false : true;
        audioTimer -= Time.deltaTime;
        Patrol();
    }

    /// <summary>
    /// 存活时将当前速度参数乘以物理帧间隔后写入水平速度，并将垂直速度设为零。
    /// </summary>
    private void FixedUpdate()
    {
        if (!healthSystem.isDead)
            rb.velocity = new Vector2(currentSpeed * Time.fixedDeltaTime, 0f);
    }

    #endregion

    #region 难度配置与巡逻

    /// <summary>
    /// 根据难度设置接触伤害与巡逻速度参数。
    /// </summary>
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配(攻击伤害|飞行速度)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                attackDamage = 2f;
                moveSpeed = 120f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                attackDamage = 3f;
                moveSpeed = 160f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                attackDamage = 4f;
                moveSpeed = 200f;
                break;
        }
    }

    /// <summary>距离下次音效播放的剩余时间，单位为秒。</summary>
    private float audioTimer;

    /// <summary>相邻音效播放的间隔，单位为秒。</summary>
    private float audioCD = 0.9f;

    /// <summary>
    /// 超出巡逻边界时改变移动方向，并按间隔播放附近可听见的振翅音效。
    /// </summary>
    private void Patrol()
    {
        //若超出了PointA的X轴范围，移动方向改为右侧
        if (transform.position.x < pointA.x)
        {
            currentSpeed = moveSpeed;
            Flip();
        }
        if (transform.position.x > pointB.x)
        {
            currentSpeed = -moveSpeed;
            Flip();
        }

        if(isAllowAudioPlay && audioTimer <= 0)
        {
            audioTimer = audioCD;
            AudioManager.Instance.PlaySound3D("Enemy_bat_wingbeat", transform.position);
        }

    }

    /// <summary>
    /// 依据当前水平移动方向翻转精灵。
    /// </summary>
    private void Flip()
    {
        if (currentSpeed > 0)
            transform.localScale = new Vector2(faceDir, transform.localScale.y);
        else
            transform.localScale = new Vector2(-faceDir, transform.localScale.y);
    }

    #endregion

    #region 接触攻击

    /// <summary>接触攻击造成的基础伤害。</summary>
    private float attackDamage = 3f;

    /// <summary>
    /// 存活时对持续接触的桌宠请求伤害及水平击退。
    /// </summary>
    /// <param name="other">持续接触的碰撞信息。</param>
    private void OnCollisionStay2D(Collision2D other)
    {
        //攻击玩家
        if (other.collider.CompareTag("Vpet") && !healthSystem.isDead)
        {
            //根据相对位置计算力的方向
            Vector2 force = transform.position.x > vpet.transform.position.x ? Vector2.left : Vector2.right;
            other.gameObject.GetComponent<VpetHealthSystem>().VpetGethurt(attackDamage, force * 300f);
        }
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 熊敌人行为类
/// - 控制熊的巡逻、追逐、随机停顿和接触攻击。
/// </summary>
public class Enemy03_Bear : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("生命系统脚本。")]
    [SerializeField] private EnemyHealthSystem healthSystem;

    [Tooltip("巡逻速度参数；写入刚体速度前会乘以物理帧间隔，并按难度初始化覆盖。")]
    [SerializeField] private float moveSpeed = 100f;

    /// <summary>巡逻点A。</summary>
    private Vector3 pointA;

    /// <summary>巡逻点B。</summary>
    private Vector3 pointB;

    /// <summary>巡逻点设置范围。</summary>
    private float pointRange = 8f;

    /// <summary>当前对象的二维刚体组件。</summary>
    private Rigidbody2D rb;

    /// <summary>当前对象的动画器。</summary>
    private Animator animator;

    /// <summary>用于交互或距离判断的桌宠对象。</summary>
    private GameObject vpet;

    /// <summary>当前移速。</summary>
    private float currentSpeed;

    /// <summary>移动方向(-1left| +1 right| 0 idle)。</summary>
    private int moveDir;

    /// <summary>初始面朝方向。</summary>
    private float faceDir;

    /// <summary>是否在移动中。</summary>
    private bool isWalking = false;

    /// <summary>是否允许音效播放。</summary>
    private bool isAllowAudioPlay = false;

    /// <summary>距离下次音效播放的剩余时间，单位为秒。</summary>
    private float audioTimer;

    [Tooltip("行走音效播放间隔，单位为秒。")]
    [SerializeField] private float audioCD = 0.8f;

    #endregion

    #region 初始化与行为更新

    /// <summary>
    /// 缓存刚体、动画器和桌宠对象。
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        vpet = GameObject.FindGameObjectWithTag("Vpet");
    }

    /// <summary>
    /// 初始化难度数值、随机移动方向、初始朝向及巡逻边界。
    /// </summary>
    void Start()
    {
        InitValueBasedDifficulty();                 //数值初始化
        moveDir = (Random.value < 0.5f) ? -1 : 1;   //初始化时给个随机方向
        faceDir = transform.localScale.x;           //默认面向方向

        pointA = new Vector3(transform.position.x - pointRange, transform.position.y); //设置PointA
        pointB = new Vector3(transform.position.x + pointRange, transform.position.y); //设置PointB

    }

    /// <summary>
    /// 根据难度设置攻击伤害、移动速度参数和随机停顿时长修正。
    /// </summary>
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配(攻击伤害|移动速度|停滞时间修正)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                attackDamage = 2f;
                moveSpeed = 75f;
                stopTimeFix = 2f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                attackDamage = 3f;
                moveSpeed = 100f;
                stopTimeFix = 0f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                attackDamage = 4f;
                moveSpeed = 125f;
                stopTimeFix = -2f;
                break;
        }
    }

    /// <summary>
    /// 更新停顿与音效计时，根据桌宠位置选择追逐或巡逻，并同步行走表现。
    /// </summary>
    void Update()
    {
        //玩家距离超过20m不播放音效
        isAllowAudioPlay = vpet != null &&
            vpet.transform != null &&
            Vector2.Distance(vpet.transform.position, transform.position) <= 20f;

        audioTimer -= Time.deltaTime;       //计时器工作
        if(isAllowStopTimerWork)  randomStopTimer -= Time.deltaTime;

        //处于移动中则更新精灵图翻转
        if (moveDir != 0)
            Flip();

        RandomStopCheck();      //随机停止监测

        float vpetPosX = vpet.transform.position.x;
        float AposX = pointA.x;
        float BposX = pointB.x;

        //若玩家进入巡逻区域，则追逐
        if (vpetPosX > AposX && vpetPosX < BposX)
            ChaseVpet();

        //否则进行巡逻
        else
            Partrol();

        isWalking = currentSpeed != 0f;
        animator.SetBool("isWalk", isWalking);

        if (isWalking && isAllowAudioPlay && audioTimer <= 0f)
        {
            audioTimer = audioCD;
            AudioManager.Instance.PlaySound3D("Enemy_bear_walk", transform.position);
        }
    }

    #endregion

    #region 停顿与巡逻追逐

    /// <summary>随机停止计时器。</summary>
    private float randomStopTimer;

    /// <summary>随机停止间隔。</summary>
    private float randomStopInterval = 10f;

    /// <summary>最低停止时长。</summary>
    private float minStopTime = 3f;

    /// <summary>最高停止时长。</summary>
    private float maxStopTime = 6f;

    /// <summary>停滞时间修正。</summary>
    private float stopTimeFix = 0f;

    /// <summary>是否暂停中。</summary>
    private bool  isStop = false;

    /// <summary>是否允许停止计时器工作。</summary>
    private bool isAllowStopTimerWork = true;

    /// <summary>
    /// 随机停顿间隔结束时暂停移动，并启动停顿等待。
    /// </summary>
    private void RandomStopCheck()
    {
        if(randomStopTimer <= 0 && !isStop)
        {
            isStop = true;
            randomStopTimer = randomStopInterval;
            moveDir = 0;
            StartCoroutine(StopTime());
        }
    }

    /// <summary>
    /// 等待受难度修正的随机时长后退出停顿，并随机选择移动方向。
    /// </summary>
    /// <returns>控制本次停顿等待的协程迭代器。</returns>
    IEnumerator StopTime()
    {
        //等待随机时长
        float rand = Random.Range(minStopTime + stopTimeFix, maxStopTime + stopTimeFix);
        yield return new WaitForSeconds(rand);
        isStop = false;
        moveDir = (Random.value < 0.5f) ? -1 : 1;   //给随机方向
    }

    /// <summary>
    /// 根据桌宠横向位置选择追逐方向；停顿期间仅在桌宠足够接近时提前恢复。
    /// </summary>
    private void ChaseVpet()
    {
        //若处于间隔暂停中且距离玩家一定距离，则不触发追逐
        if (isStop && Vector2.Distance(vpet.transform.position, transform.position) > 2f) return;
        else if (isStop)
        {
            isStop = false;
        }
        isAllowStopTimerWork = false;
        moveDir = vpet.transform.position.x > transform.position.x ? 1 : -1;    //根据vpet位置设置移动方向

    }

    /// <summary>
    /// 非停顿时启用停顿间隔计时，并在越过巡逻边界时反向移动。
    /// </summary>
    private void Partrol()
    {
        if (isStop) return;
        isAllowStopTimerWork = true;
        //边界监测(超过巡逻范围则会强制反向移动)
        if (transform.position.x <= pointA.x && moveDir < 0)
        {
            moveDir = 1;  // 到达左边界，向右移动
        }
        else if (transform.position.x >= pointB.x && moveDir > 0)
        {
            moveDir = -1; // 到达右边界，向左移动
        }
    }

    #endregion

    #region 物理运动与接触攻击

    /// <summary>
    /// 存活时按移动方向写入水平速度，保留现有垂直速度。
    /// </summary>
    void FixedUpdate()
    {
        if (healthSystem.isDead) return;

        currentSpeed = moveDir * moveSpeed;   //设置移动速度

        //怪物移动
        if (currentSpeed != 0f)
            rb.velocity = new Vector2(currentSpeed * Time.fixedDeltaTime, rb.velocity.y);
        else
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    /// <summary>
    /// 依据非零移动方向和初始缩放翻转精灵。
    /// </summary>
    private void Flip()
    {
        if (moveDir > 0)
            transform.localScale = new Vector2(faceDir, transform.localScale.y);
        else if (moveDir < 0)
            transform.localScale = new Vector2(-faceDir, transform.localScale.y);
    }

    /// <summary>接触攻击造成的基础伤害。</summary>
    private float attackDamage = 3f;

    /// <summary>
    /// 存活时对持续接触的桌宠请求伤害及斜向击退。
    /// </summary>
    /// <param name="other">持续接触的碰撞信息。</param>
    private void OnCollisionStay2D(Collision2D other)
    {
        //攻击玩家
        if (other.collider.CompareTag("Vpet") && !healthSystem.isDead)
        {
            //根据相对位置计算力的方向
            Vector2 force = transform.position.x > vpet.transform.position.x ? Vector2.left : Vector2.right;
            other.gameObject.GetComponent<VpetHealthSystem>().VpetGethurt(attackDamage, force * 400f + Vector2.up * 100f);
        }
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 青蛙敌人行为类
/// - 控制青蛙跳跃巡逻、追逐、落地判定和跳跃期间的接触攻击。
/// </summary>
public class Enemy01_Frog : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>当前对象的动画器。</summary>
    private Animator animator;

    /// <summary>当前对象的二维刚体组件。</summary>
    private Rigidbody2D rb;

    /// <summary>起跳时直接写入的垂直速度，而非施加的力。</summary>
    private float jumpForceY = 5f;

    /// <summary>沿水平方向施加的起跳冲量大小。</summary>
    private float jumpForceX = 5f;

    [Tooltip("生命系统脚本。")]
    [SerializeField] private EnemyHealthSystem healthSystem;

    /// <summary>巡逻点1。</summary>
    private Vector3 pointA;

    /// <summary>巡逻点2。</summary>
    private Vector3 pointB;

    /// <summary>巡逻点设置范围。</summary>
    private float pointRange = 5f;

    /// <summary>桌宠横坐标是否位于巡逻区间内，用于决定追逐。</summary>
    private bool isVpetInRange = false;

    /// <summary>是否处于允许接触攻击的跳跃阶段；初始值为 true。</summary>
    private bool isAttacking = true;

    /// <summary>唯一面向向量值。</summary>
    private float faceDir;

    /// <summary>用于交互或距离判断的桌宠对象。</summary>
    private GameObject vpet;

    #endregion

    #region 生命周期

    /// <summary>
    /// 缓存动画器、刚体及桌宠对象引用。
    /// </summary>
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //获取桌宠的游戏对象
    }

    /// <summary>
    /// 记录初始朝向和巡逻边界，按难度设置伤害并随机选择跳跃间隔。
    /// </summary>
    private void Start()
    {
        faceDir = transform.localScale.x;
        pointA = new Vector3(transform.position.x - pointRange, transform.position.y); //设置PointA
        pointB = new Vector3(transform.position.x + pointRange, transform.position.y); //设置PointB

        InitValueBasedDifficulty();     //初始化难度相关数值
        jumpCD = Random.Range(1f, 2f);  //随机跳跃CD

    }

    /// <summary>
    /// 更新落地与追逐判定、巡逻跳跃、音效及允许运行的冷却计时器。
    /// </summary>
    private void Update()
    {
        if (healthSystem.isDead) return;    //若已死亡则不执行

        CheckIsGrounded();      //落地监测
        CheckFall();            //坠落监测
        CheckHasFallGround();   //是否已落地监测
        CheckVpetEnter();       //监测桌宠是否进入攻击范围
        FrogIdleAudio();        //青蛙待机音效

        if (isVpetInRange)      //玩家在攻击范围内执行追逐行为
            ChaseVpet();
        else
            Patrol();           //否则执行巡逻行为

        if (isAllowTimerWork)
        {
            jumpTimer -= Time.deltaTime;
            frogYellTimer -= Time.deltaTime;
        }

        //与玩家距离超过20f后禁止播放音效
        isAllowAudioPlay = Vector2.Distance(vpet.transform.position, transform.position) > 20f ? false : true;
    }

    #endregion

    #region 难度与巡逻跳跃

    /// <summary>
    /// 根据当前难度设置接触攻击伤害。
    /// </summary>
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配(伤害)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                attackDamage = 2f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                attackDamage = 3f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                attackDamage = 4f;
                break;
        }
    }

    /// <summary>跳跃计时器。</summary>
    private float jumpTimer;

    /// <summary>跳跃CD。</summary>
    private float jumpCD;

    /// <summary>是否允许递减跳跃及叫声计时器。</summary>
    private bool isAllowTimerWork = true;

    /// <summary>当前巡逻或追逐选择的水平起跳方向。</summary>
    Vector2 jumpDir = Vector2.right;

    /// <summary>是否允许播放音频。</summary>
    private bool isAllowAudioPlay = true;

    /// <summary>
    /// 在巡逻左右边界之间调整跳跃方向，并在冷却结束时尝试起跳。
    /// </summary>
    private void Patrol()
    {
        //若超出了PointA的X轴范围，力方向更改为右侧
        if (transform.position.x < pointA.x)
            jumpDir = Vector2.right;

        if (transform.position.x > pointB.x)
            jumpDir = Vector2.left;

        if (jumpTimer <= 0)
            Jump(jumpDir);

    }

    /// <summary>
    /// 朝桌宠所在的水平方向移动，并在冷却结束时尝试起跳。
    /// </summary>
    private void ChaseVpet()
    {
        if (vpet == null) return;

        //计算青蛙与玩家的相对位置
        float pos = transform.position.x - vpet.transform.position.x;
        //若玩家在青蛙左侧
        if (pos > 0)
            jumpDir = Vector2.left;
        else
            jumpDir = Vector2.right;

        if (jumpTimer <= 0)
            Jump(jumpDir);
    }

    /// <summary>
    /// 接地时设置垂直速度并施加水平冲量，暂停冷却计时并进入攻击阶段。
    /// </summary>
    /// <param name="jumpDir">本次起跳使用的水平施力方向。</param>
    private void Jump(Vector2 jumpDir)
    {
        if (!isGrounded) return;    //处于地面时才能跳跃
        isAttacking = true;         //攻击中
        isAllowTimerWork = false;   //期间禁止计时
        isAllowCheckFall = true;    //允许进行落地监测
        jumpTimer = jumpCD;         //CD重置

        rb.velocity = new Vector2(rb.velocity.x, jumpForceY);  //赋予y向速度
        rb.AddForce(jumpDir * jumpForceX, ForceMode2D.Impulse); //添加跳跃力
        Flip(); //进行朝向矫正

        animator.SetTrigger("jump");        //动画播放

        if (isAllowAudioPlay)
            AudioManager.Instance.PlaySound3D("Enemy_frog_jump", transform.position);    //播放音效
    }

    /// <summary>
    /// 根据桌宠横坐标是否位于巡逻边界内更新追逐标记，不检测垂直距离。
    /// </summary>
    private void CheckVpetEnter()
    {
        //若桌宠的X坐标在pointA与pointB区间
        if (vpet.transform.position.x > pointA.x &&
           vpet.transform.position.x < pointB.x)
        {
            isVpetInRange = true;
        }
        else isVpetInRange = false;
    }

    #endregion

    #region 落地判定与朝向

    /// <summary>是否落地。</summary>
    private bool isGrounded = true;

    /// <summary>射线长度。</summary>
    private float rayLength = 0.1f;

    /// <summary>射线半宽间隔。</summary>
    private float halfWidth = 0.46f;

    [Tooltip("地面&&敌人层。")]
    [SerializeField] private LayerMask Layer;

    /// <summary>
    /// 通过三条向下射线判断是否接近地面，并同步动画器的接地参数。
    /// </summary>
    private void CheckIsGrounded()
    {
        // 三个射线起点：中、左、右
        Vector2 centerOrigin = transform.position + Vector3.down *0.3f;
        Vector2 leftOrigin = centerOrigin + Vector2.left * halfWidth;
        Vector2 rightOrigin = centerOrigin + Vector2.right * halfWidth;
        isGrounded = false;

        // 依次发射三条向下射线，使用 groundLayer 过滤
        foreach (Vector2 origin in new[] { centerOrigin, leftOrigin, rightOrigin })
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, Layer);
            if (hit.collider != null)
            {
                isGrounded = true;
                break;
            }
        }
        animator.SetBool("isGround", isGrounded);   //与动画器同步
    }

    /// <summary>是否允许检测坠落。</summary>
    private bool isAllowCheckFall = false;

    /// <summary>
    /// 起跳后在垂直速度低于阈值时触发下落动画，并开始等待落地。
    /// </summary>
    private void CheckFall()
    {
        //若允许监测
        if (isAllowCheckFall)
        {
            //当y向速度低于一定阈值则触发坠落动作
            if (rb.velocity.y < 0.1)
            {
                isAllowCheckFall = false;
                animator.SetTrigger("fall");
                hasFallGround = false;
            }
        }
    }

    /// <summary>本轮下落是否已确认接地，用于恢复计时并关闭攻击阶段。</summary>
    private bool hasFallGround = true;

    /// <summary>
    /// 确认下落后的首次接地，恢复冷却计时并关闭攻击标记。
    /// </summary>
    private void CheckHasFallGround()
    {
        if (!hasFallGround)
        {
            //若此时已落地
            if (isGrounded)
            {
                hasFallGround = true;       //已落地
                isAllowTimerWork = true;    //允许计时器工作
                isAttacking = false;        //关闭攻击状态
            }
        }
    }

    /// <summary>
    /// 根据当前跳跃方向和初始缩放翻转精灵朝向。
    /// </summary>
    private void Flip()
    {
        if(jumpDir == Vector2.right)
             transform.localScale = new Vector2(-faceDir, transform.localScale.y);
        else
            transform.localScale = new Vector2(faceDir, transform.localScale.y);
    }

    #endregion

    #region 音效与接触攻击

    /// <summary>距离下次待机叫声的剩余时间，单位为秒。</summary>
    private float frogYellTimer;

    /// <summary>待机叫声的最短播放间隔，单位为秒。</summary>
    private float frogYellCD = 1.3f;

    /// <summary>
    /// 在跳跃间隔内按冷却播放待机叫声；远离桌宠时不播放。
    /// </summary>
    private void FrogIdleAudio()
    {
        if (!isAllowAudioPlay) return;

        if (jumpTimer > 0 && jumpTimer < jumpCD - 0.5f)
        {
            if (frogYellTimer < 0)
            {
                frogYellTimer = frogYellCD;
                AudioManager.Instance.PlaySound3D("Enemy_frog_idle",transform.position);
            }
        }

    }

    /// <summary>接触攻击造成的基础伤害。</summary>
    private float attackDamage = 3f;

    /// <summary>
    /// 处于攻击阶段且未死亡时，对持续接触的桌宠请求伤害和水平击退。
    /// </summary>
    /// <param name="other">持续接触的碰撞信息。</param>
    private void OnCollisionStay2D(Collision2D other)
    {
        //攻击玩家
        if (other.collider.CompareTag("Vpet") && isAttacking && !healthSystem.isDead)
        {
            //根据相对位置计算力的方向
            Vector2 force = transform.position.x > vpet.transform.position.x ? Vector2.left : Vector2.right;
            other.gameObject.GetComponent<VpetHealthSystem>().VpetGethurt(attackDamage, force * 200f);
        }
    }

    #endregion
}

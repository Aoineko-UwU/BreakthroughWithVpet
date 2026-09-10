using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 桌宠状态枚举
/// - 定义桌宠行为及结算状态；编号同时供外部状态切换入口使用。
/// </summary>
public enum VpetState
{
    /// <summary>待机状态，外部编号为 0。</summary>
    Idle,
    /// <summary>行走状态，外部编号为 1。</summary>
    Walking,
    /// <summary>飘飞状态，外部编号为 2。</summary>
    Fall,
    /// <summary>攀爬状态，外部编号为 3。</summary>
    Climb,
    /// <summary>进食状态，外部编号为 4。</summary>
    Eat,
    /// <summary>睡眠状态，外部编号为 5。</summary>
    Sleep,
    /// <summary>跳舞状态，外部编号为 6。</summary>
    Dance,
    /// <summary>死亡状态，外部编号为 7。</summary>
    Die,
    /// <summary>胜利状态，外部编号为 8。</summary>
    Win
}

/// <summary>
/// 桌宠行为类
/// - 保留外部入口与行为协调，将环境查询和物理操作委托给专用对象。
/// </summary>
public class VpetAction : MonoBehaviour
{
    #region 组件引用与运行状态

    [Tooltip("桌宠主体动画器，驱动行走、攀爬、进食和结算动画。")]
    [SerializeField] private Animator _animatorVpet;
    [Tooltip("桌宠手部动画器，与主体同步播放进食动画。")]
    [SerializeField] private Animator _animatorVpetHand;
    [Tooltip("被食用物品的动画器，播放进食过程中的物品动画。")]
    [SerializeField] private Animator _animatorEatenItem;

    [Tooltip("炸弹预制体")]
    [SerializeField] private GameObject bombPrefab;
    [Tooltip("胜利触发粒子")]
    [SerializeField] private GameObject winParticle;
    [Tooltip("加速Buff粒子")]
    [SerializeField] private GameObject speedUpParticle;
    [Tooltip("伤害Buff粒子")]
    [SerializeField] private GameObject attackUpParticle;

    [Tooltip("一拳状态")]
    [SerializeField] private GameObject onePunchState;

    /// <summary>负责地面接触、射线与瞬移重叠查询的协作对象。</summary>
    private VpetEnvironmentSensor environmentSensor;

    /// <summary>负责施力、位置和碰撞体操作的协作对象。</summary>
    private VpetRigMotion rigMotion;

    /// <summary>桌宠生命系统。</summary>
    private VpetHealthSystem health;

    /// <summary>桌宠状态。</summary>
    private VpetState currentState;

    #endregion

    #region 生命周期

    /// <summary>
    /// 缓存组件并创建环境与运动协作对象，初始化待机状态和提示画布。
    /// </summary>
    private void Awake()
    {
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        ConstantForce2D floatingForce = GetComponent<ConstantForce2D>();
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        health = GetComponent<VpetHealthSystem>();
        currentState = VpetState.Idle;

        // 协作对象只执行显式调用，不引入额外的 Unity 生命周期顺序。
        environmentSensor = new VpetEnvironmentSensor(body, capsule);
        rigMotion = new VpetRigMotion(body, floatingForce, capsule);
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");
    }

    /// <summary>
    /// 初始化碰撞体形状、接触过滤器及当前难度对应的战斗数值。
    /// </summary>
    private void Start()
    {
        VpetColliderChange();               //初始化碰撞箱
        environmentSensor.InitializeContactFilters(groundLayer, allGroundLayer);
        InitValueBasedDifficulty();         //初始化难度相关数值
    }

    /// <summary>
    /// 在物理帧内更新行走、攀爬和飘飞水平驱动力；结算停用后不再执行。
    /// </summary>
    private void FixedUpdate()
    {

        if (health.isVpetDead) return;      //桌宠死亡则不执行
        VpetWalk();     //桌宠移动行为
        VpetClimb();    //桌宠攀爬行为
        VpetFallHorSpeedSet();  //桌宠飘飞水平力
    }

    /// <summary>
    /// 逐帧更新飘飞流程、地面探测、跳舞效果和各行为计时器。
    /// </summary>
    private void Update()
    {
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        VpetFall();         //桌宠飘飞行为
        VpetFallCheck();    //桌宠飘飞行为监测
        VpetDance();        //桌宠跳舞行为
        TimerWork();        //计时器工作
    }

    #endregion

    #region 难度配置

    /// <summary>
    /// 根据当前难度设置普通攻击伤害、攻击间隔和尖刺伤害。
    /// </summary>
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                vpetAttackDamage = 4f;
                vpetAttackCD = 1.2f;
                spikeDamage = 2f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                vpetAttackDamage = 3f;
                vpetAttackCD = 1.3f;
                spikeDamage = 3f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                vpetAttackDamage = 2f;
                vpetAttackCD = 1.4f;
                spikeDamage = 4f;
                break;
        }
    }

    #endregion

    #region 行走与地面接触

    /// <summary>行走及飘飞水平驱动力的增益倍率。</summary>
    private float speedUpBuffFix = 1f;

    /// <summary>行走或游泳音效距离下次播放的剩余时间，单位为秒。</summary>
    private float walkAudioTimer;
    /// <summary>行走或游泳音效的播放间隔，单位为秒。</summary>
    private float walkAudioCD = 0.62f;
    /// <summary>
    /// 仅在行走状态下沿接触面切线施加驱动力，并同步行走动画与音效。
    /// <para>水面探测、地面接触和加速增益共同影响施力大小；该增益不直接修改目标速度。</para>
    /// </summary>
    private void VpetWalk()
    {
        //桌宠状态为行走时执行移动
        if (currentState == VpetState.Walking)
        {
            //走路音效播放
            if (walkAudioTimer <= 0)
            {
                walkAudioTimer = walkAudioCD;
                if (environmentSensor.IsInWater)
                    AudioManager.Instance.PlaySound("swim");
                else
                    AudioManager.Instance.PlaySound("walk");
            }

            _animatorVpet.SetBool("isWalk", true);

            // 探测器提供环境信息，运动对象只负责沿用现有施力算法。
            rigMotion.Walk(
                environmentSensor.GetAverageGroundNormal(),
                environmentSensor.IsInWater,
                environmentSensor.IsInWater ? false : environmentSensor.HasGroundContact(),
                environmentSensor.IsGrounded,
                speedUpBuffFix);
        }
        else
        {
            _animatorVpet.SetBool("isWalk", false);
        }
    }

    [Tooltip("仅地面层级")]
    [SerializeField] LayerMask groundLayer;

    [Tooltip("所有地面有关层级")]
    [SerializeField] LayerMask allGroundLayer;

    #endregion

    #region 攀爬行为

    /// <summary>是否正在攀爬。</summary>
    private bool isClimbing = false;

    /// <summary>攀爬音效计时器。</summary>
    private float climbAudioTimer;
    /// <summary>攀爬音效间隔。</summary>
    private float climbAudioCD = 0.62f;
    /// <summary>
    /// 在攀爬状态下施加向上驱动力，并处理攀爬动画切换与间隔音效。
    /// </summary>
    private void VpetClimb()
    {
        _animatorVpet.SetBool("isClimbing", isClimbing);    //动画机状态同步
        //桌宠状态为攀爬时执行
        if (currentState == VpetState.Climb)
        {
            _animatorVpet.ResetTrigger("ClimbEnd");         //重置ClimbEnd Trigger

            //若此时不是正在攀爬，则执行一次动画
            if (!isClimbing)
            {
                isClimbing = true;
                rigMotion.ResetVelocity();
                _animatorVpet.SetTrigger("ClimbStart");
            }

            rigMotion.Climb();

            //攀爬音效播放(随机)
            if (climbAudioTimer <= 0)
            {
                climbAudioTimer = climbAudioCD;
                int rand = Random.Range(1, 5);
                AudioManager.Instance.PlaySound($"Assets/Audio/Vpet/climb/ladder" + rand + ".wav");
            }
        }
        else
        {
            if (isClimbing)
            {
                isClimbing = false;
                _animatorVpet.SetTrigger("ClimbEnd");
            }
        }
    }

    #endregion

    #region 进食与食物效果

    [Tooltip("进食道具Sprite渲染器绑定")]
    [SerializeField] SpriteRenderer eatenItemSprite;
    [Tooltip("一拳状态粒子")]
    [SerializeField] GameObject onePunchEffect;
    /// <summary>是否允许开始新的进食流程；由当前行为阶段控制。</summary>
    [Tooltip("是否允许开始新的进食流程；运行时由行为阶段更新。")]
    public bool isAllowEat = true;
    /// <summary>
    /// 在桌宠存活且允许进食时播放进食表现，并延迟执行食物效果。
    /// </summary>
    /// <param name="item">待食用的物品数据；为空时不启动进食流程。</param>
    public void VpetEat(ItemData item)
    {
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        //非空检查
        if (item != null && isAllowEat)
        {
            if (isFalling) StopFallingLogic();      //若处于坠落状态，则停止坠落逻辑

            isAllowEat = false;                     //更改为不允许再进食
            eatenItemSprite.sprite = item.icon;     //改变食物精灵图
            currentState = VpetState.Eat;           //改变桌宠当前状态

            //播放进食动画
            _animatorVpet.SetTrigger("Eat");
            _animatorVpetHand.SetTrigger("Eat");
            _animatorEatenItem.SetTrigger("Eat");

            AudioManager.Instance.PlaySound("pickItem");    //播放音效

            //启用协程延迟时间后判断食物类型
            StartCoroutine(StartFoodJudge(item));

            //重置攀爬状态
            if (isClimbing)
                isClimbing = false;

            //重置飘飞状态
            if (isFalling)
                isFalling = false;

        }
    }

    /// <summary>
    /// 等待进食动画阶段结束，再按物品编号分派恢复、睡眠、增益或随机事件。
    /// </summary>
    /// <param name="item">由进食入口传入的非空物品数据。</param>
    /// <returns>包含进食等待阶段的协程迭代器。</returns>
    IEnumerator StartFoodJudge(ItemData item)
    {
        yield return new WaitForSeconds(0.8f);
        AudioManager.Instance.PlaySound("eating");  //播放吃东西音效
        yield return new WaitForSeconds(1.6f);

        int index = item.itemID;    //获取物品ID

        //根据物品ID判断执行不同效果
        switch (index)
        {
            //金苹果
            case 0:
                health.VpetRecover(10f);   //回复生命
                currentState = VpetState.Walking;                   //更新桌宠状态
                isAllowEat = true;
                break;

            //昏睡红茶
            case 1:
                StartCoroutine(VpetSleep());    //桌宠睡眠
                break;

            //肾宝
            case 2:
                isOnePunch = true;              //强化下一次攻击
                onePunchState.SetActive(true);  //设置Effect状态图
                Instantiate(onePunchEffect, transform.position, Quaternion.identity);
                AudioManager.Instance.PlaySound("OnePunchState");
                currentState = VpetState.Walking;
                isAllowEat = true;
                break;

            //地球
            case 3:
                int eventIndex = RandomSelector.Instance.EventRandomSelector(1);    //获取随机的事件
                switch(eventIndex)
                {
                    //生命恢复事件
                    case 1:
                        health.VpetRecover(20f);
                        currentState = VpetState.Walking;
                        break;
                    //瞬间死亡
                    case 2:
                        health.VpetGethurt(999f, Vector2.up * 100f);
                        currentState = VpetState.Die;
                        break;
                    //移动加速
                    case 3:
                        if(speedUpBuffCoroutine != null)
                        {
                            StopCoroutine(speedUpBuffCoroutine);
                            speedUpBuffCoroutine = StartCoroutine(Eat_SpeedUp());
                        }
                        else
                            speedUpBuffCoroutine = StartCoroutine(Eat_SpeedUp());

                        currentState = VpetState.Walking;
                        break;
                    //普通攻击伤害增加，攻击频率加快
                    case 4:
                        if(AttackUpBuffCoroutine != null)
                        {
                            StopCoroutine(AttackUpBuffCoroutine);
                            AttackUpBuffCoroutine = StartCoroutine(Eat_AttackUp());
                        }
                        else
                            AttackUpBuffCoroutine = StartCoroutine(Eat_AttackUp());

                        currentState = VpetState.Walking;
                        break;
                    //扣除生命
                    case 5:
                        health.VpetGethurt(10f, Vector2.up * 100f);
                        currentState = VpetState.Walking;
                        break;
                    //瞬移
                    case 6:
                        Teleport();
                        currentState = VpetState.Walking;
                        break;
                    //瞬间爆炸
                    case 7:
                        var bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
                        bomb.GetComponent<Item_Block_bomb>().isInstanctlyExplode = true;
                        currentState = VpetState.Walking;
                        break;
                };

                isAllowEat = true;
                break;

            //可乐(加伤害)
            case 4:
                if (AttackUpBuffCoroutine != null)
                {
                    StopCoroutine(AttackUpBuffCoroutine);
                    AttackUpBuffCoroutine = StartCoroutine(Eat_AttackUp());
                }
                else
                    AttackUpBuffCoroutine = StartCoroutine(Eat_AttackUp());

                currentState = VpetState.Walking;
                isAllowEat = true;
                break;

            //雪碧(加速)
            case 5:
                if (speedUpBuffCoroutine != null)
                {
                    StopCoroutine(speedUpBuffCoroutine);
                    speedUpBuffCoroutine = StartCoroutine(Eat_SpeedUp());
                }
                else
                    speedUpBuffCoroutine = StartCoroutine(Eat_SpeedUp());
                currentState = VpetState.Walking;
                isAllowEat = true;
                break;

            default:
                Debug.Log("未知食物类型");
                break;
        }

    }

    #endregion

    #region 限时增益

    /// <summary>加速Buff给予的加速倍率。</summary>
    float speedUpBuffMultiplier = 1.7f;
    /// <summary>加速持续时间。</summary>
    float speedUpBuffDuration = 12f;
    /// <summary>加速Buff协程。</summary>
    private Coroutine speedUpBuffCoroutine;
    /// <summary>
    /// 启用行走与飘飞水平施力增益，生成提示和粒子，并在持续时间结束后还原倍率。
    /// </summary>
    /// <returns>控制加速增益持续时间的协程迭代器。</returns>
    IEnumerator Eat_SpeedUp()
    {
        ShowText("速度提升↑↑");
        AudioManager.Instance.PlaySound("getBuff");
        speedUpBuffFix = speedUpBuffMultiplier; //更改加速倍率
        var par = Instantiate(speedUpParticle, transform.position, Quaternion.identity, transform); //粒子生成
        Destroy(par, speedUpBuffDuration);
        //等待效果持续时间
        yield return new WaitForSeconds(speedUpBuffDuration);

        speedUpBuffFix = 1f;            //恢复默认倍率
        speedUpBuffCoroutine = null;    //清理本协程
    }

    /// <summary>攻击Buff给予的攻击倍率。</summary>
    float attackBuffDamageMultiplier = 2f;
    /// <summary>攻击Buff给予的攻击间隔倍率。</summary>
    float attackBuffTimeMultiplier = 0.5f;
    /// <summary>攻击Buff的持续时间。</summary>
    float attackBuffDuration = 12f;
    /// <summary>攻击Buff协程。</summary>
    private Coroutine AttackUpBuffCoroutine;
    /// <summary>
    /// 临时提高普通攻击伤害、缩短攻击间隔并禁止受击击退，结束后恢复默认修正。
    /// </summary>
    /// <returns>控制攻击增益持续时间的协程迭代器。</returns>
    IEnumerator Eat_AttackUp()
    {
        AudioManager.Instance.PlaySound("getBuff");
        ShowText("攻击提升↑↑");
        health.SetKnockBack(false);     //不可击退状态
        var par = Instantiate(attackUpParticle, transform.position, Quaternion.identity, transform); //粒子生成
        Destroy(par, speedUpBuffDuration);
        //更改倍率
        attackBuffDamageFix = attackBuffDamageMultiplier;
        attackBuffTimeFix = attackBuffTimeMultiplier;
        //等待效果持续时间
        yield return new WaitForSeconds(attackBuffDuration);
        //恢复默认倍率
        attackBuffDamageFix = 1f;
        attackBuffTimeFix = 1f;
        health.SetKnockBack(true);      //可击退状态

        AttackUpBuffCoroutine = null;   //清理本协程
    }

    #endregion

    #region 随机瞬移

    /// <summary>最大瞬移范围。</summary>
    private float teleportRange = 12f;
    /// <summary>最大查找次数。</summary>
    private int maxSearchAttempts = 10;
    /// <summary>
    /// 在限定范围和尝试次数内寻找可用瞬移位置，并播放音效。
    /// <para>若所有候选位置均被拒绝，则将当前位置向上移动 0.5 个世界单位。</para>
    /// </summary>
    private void Teleport()
    {
        Vector2 targetPosition = Vector2.zero;
        bool foundValidPosition = false;

        // 尝试查找最多 maxSearchAttempts 次
        for (int i = 0; i < maxSearchAttempts; i++)
        {
            // 随机选择一个目标位置（在指定范围内）
            Vector2 randomDirection = Random.insideUnitCircle * teleportRange;  // 在一个圆形范围内随机
            targetPosition = (Vector2)transform.position + randomDirection;

            // 检查目标位置是否有效
            if (environmentSensor.CanTeleportTo(targetPosition))
            {
                foundValidPosition = true;  // 找到了有效的目标位置
                break;  // 退出查找
            }
        }
        // 找到候选位置则移动到该位置；查找失败时向上偏移 0.5 个单位。
        if (foundValidPosition)
            rigMotion.TeleportTo(targetPosition);
        else
            rigMotion.TeleportTo(transform.position + Vector3.up * 0.5f);

        AudioManager.Instance.PlaySound("teleport");
    }

    //// 使用Gizmo绘制出目标地点的监测半径
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red; // 监测范围的颜色
    //    Gizmos.DrawWireSphere(transform.position, teleportRange);  // 绘制绿色的圆形区域
    //}

    #endregion

    #region 睡眠行为

    /// <summary>进入睡眠循环后的持续时间，不含入睡和起身动画等待。</summary>
    private float sleepTime = 7.5f;
    /// <summary>睡眠音效距离下次播放的剩余时间，单位为秒。</summary>
    private float sleepAudioTimer;
    /// <summary>睡眠音效的播放间隔，单位为秒。</summary>
    private float sleepAudioTimerCD = 2.3f;

    /// <summary>睡眠恢复计时器。</summary>
    private float sleepRecoverTimer;
    /// <summary>睡眠恢复间隔。</summary>
    private float sleepRecoverCD = 1f;
    /// <summary>每次生命恢复量。</summary>
    private float sleepRecoverRate = 1f;
    /// <summary>
    /// 播放入睡和起身动画，在睡眠期间定时恢复生命，结束后恢复行走及进食权限。
    /// </summary>
    /// <returns>串联入睡、睡眠和起身阶段的协程迭代器。</returns>
    IEnumerator VpetSleep()
    {
        currentState = VpetState.Sleep;
        AudioManager.Instance.PlaySound("startSleep");  //音效播放
        _animatorVpet.SetTrigger("SleepStart");         //睡眠动作播放

        yield return new WaitForSeconds(2.5f);          //等待动作播放

        VpetColliderChange();                           //改变碰撞箱

        float elapsedTime = 0f;     //经过时间
        while (elapsedTime < sleepTime)
        {
            elapsedTime += Time.deltaTime;
            if (sleepAudioTimer <= 0)
            {
                sleepAudioTimer = sleepAudioTimerCD;
                AudioManager.Instance.PlaySound("sleeping");
            }

            if (sleepRecoverTimer <= 0)
            {
                sleepRecoverTimer = sleepRecoverCD;
                health.VpetRecover(sleepRecoverRate);
            }

            yield return null; // 等待下一帧，保持循环活跃
        }

        AudioManager.Instance.StopSound("sleeping");
        _animatorVpet.SetTrigger("SleepEnd");           //起身动作播放
        yield return new WaitForSeconds(0.9f);          //等待动作播放

        currentState = VpetState.Walking;               //更改为行走状态
        VpetColliderChange();                           //更改碰撞箱
        isAllowEat = true;                              //允许进食
    }

    #endregion

    #region 地面探测与飘飞

    /// <summary>是否允许坠落监测计时器工作。</summary>
    private bool isAllowFallCheckTimer = false;
    /// <summary>离地确认的剩余等待时间，单位为秒。</summary>
    private float fallConfirmTimer;
    /// <summary>坠落监测间隔(超过这个时间不处于地面则判定为坠落中)。</summary>
    private float fallConfirmInterval = 0.2f;

    /// <summary>
    /// 依次向下探测中、左、右三个位置，并通过延迟确认切换飘飞状态。
    /// <para>地面和水面标记以首条命中射线为准，不等同于刚体接触或浸水检测。</para>
    /// </summary>
    private void VpetFallCheck()
    {
        environmentSensor.RefreshGround(transform.position, allGroundLayer);

        // 从行走切到坠落
        if (currentState == VpetState.Walking || currentState == VpetState.Idle)
        {
            //启用掉落状态监测
            if (!isAllowFallCheckTimer && !environmentSensor.IsGrounded)
            {
                fallConfirmTimer = fallConfirmInterval; //赋予时间
                isAllowFallCheckTimer = true;           //开始进行监测
            }
        }
        //若启用了掉落状态监测计时器&&计时完成
        if (isAllowFallCheckTimer && fallConfirmTimer <= 0)
        {
            //计时结束时若还处在空中
            if (!environmentSensor.IsGrounded)
            {
                currentState = VpetState.Fall;      //确认转换为飘飞状态
                isAllowFallCheckTimer = false;      //停止计时器使用
            }
            else
                isAllowFallCheckTimer = false;      //停止计时器使用
        }
    }

    /// <summary>是否正在坠落。</summary>
    private bool isFalling = false;

    /// <summary>飘飞音效计时器。</summary>
    private float fallAudioTimer;
    /// <summary>飘飞音效间隔。</summary>
    private float fallAudioCD = 1f;
    /// <summary>
    /// 处理飘飞开始时的持续升力与表现，并在落地后启动起身等待。
    /// </summary>
    private void VpetFall()
    {
        //桌宠状态为坠落时触发
        if(currentState == VpetState.Fall && !environmentSensor.IsGrounded && !isGetUpCoroutineWork)
        {
            //执行一次
            if (!isFalling)
            {
                isFalling = true;                               //设置为正在下落
                _animatorVpet.SetBool("isFalling", isFalling);  //更新动画器参数
                _animatorVpet.SetTrigger("FallStart");          //播放一次动画
                AudioManager.Instance.PlaySound("startFall");   //播放一次音效
                rigMotion.StartFloating();
            }

            //飘飞音效播放
            if (fallAudioTimer <= 0 && isFalling)
            {
                fallAudioTimer = fallAudioCD;
                AudioManager.Instance.PlaySound("fall");
            }
        }

        //若已落地
        if(environmentSensor.IsGrounded && isFalling && currentState == VpetState.Fall && !isGetUpCoroutineWork)
        {
            isAllowEat = false;                         //禁止进食
            StopFallingLogic();                         //坠落停止逻辑
            StartCoroutine(VpetGetUp());                //起身延迟
            AudioManager.Instance.PlaySound("fallen");  //音效播放
        }
    }

    /// <summary>
    /// 在空中飘飞时依据水平速度差施力；向左运动时减弱向右修正。
    /// </summary>
    private void VpetFallHorSpeedSet()
    {
        if (currentState == VpetState.Fall && !environmentSensor.IsGrounded && isFalling)
        {
            rigMotion.DriveFloatingHorizontal(speedUpBuffFix);

        }
    }

    /// <summary>起身协程是否正在等待，用于防止重复启动。</summary>
    bool isGetUpCoroutineWork = false;
    /// <summary>
    /// 等待落地起身阶段结束，再恢复行走状态和进食权限。
    /// </summary>
    /// <returns>控制起身等待及流程标记的协程迭代器。</returns>
    IEnumerator VpetGetUp()
    {
        isGetUpCoroutineWork = true;
        yield return new WaitForSeconds(2.5f);
        isAllowEat = true;
        currentState = VpetState.Walking;
        isGetUpCoroutineWork = false;
    }

    /// <summary>
    /// 清除飘飞标记、落地确认计时开关、持续力以及飘飞动画和循环音效。
    /// </summary>
    private void StopFallingLogic()
    {
        isFalling = false;                              //变更为不在掉落中
        _animatorVpet.SetBool("isFalling", isFalling);  //更新动画器参数
        isAllowFallCheckTimer = false;
        rigMotion.StopFloating();                     //仅停止持续力，保留当前速度
        AudioManager.Instance.StopSound("fall");      //终止音效播放
    }

    #endregion

    #region 跳舞行为

    /// <summary>是否正在跳舞。</summary>
    private bool isVpetDancing = false;
    /// <summary>跳舞生命回复计时器。</summary>
    private float vpetDanceRecoverTimer;
    /// <summary>计时器CD。</summary>
    private float vpetDanceRecoverCD = 1f;
    /// <summary>每次恢复量。</summary>
    private float recoverPerDance = 1f;

    /// <summary>跳舞伤害计时器。</summary>
    private float vpetDanceAttackTimer;
    /// <summary>计时器CD。</summary>
    private float vpetDanceAttackCD = 0.5f;
    /// <summary>每次伤害量。</summary>
    private float damagePerDance = 3f;
    /// <summary>伤害半径。</summary>
    private float damagePerDanceRadius = 2f;
    [Tooltip("敌人层")]
    [SerializeField] private LayerMask enemyLayer;
    /// <summary>
    /// 在跳舞状态下启用无敌，并按间隔恢复生命、对附近敌人造成范围伤害。
    /// </summary>
    private void VpetDance()
    {
        if(currentState == VpetState.Dance)
        {
            isAllowEat = false;                 //禁止进食
            health.isVpetInvincible = true;     //无敌效果

            if (!isVpetDancing)
            {
                isVpetDancing = true;
                _animatorVpet.SetTrigger("Dance");                  //触发动作
                AudioManager.Instance.PlayBGM("DanceMusic");        //播放音乐
                StartCoroutine(DanceTime());
            }

            //生命恢复
            if (vpetDanceRecoverTimer <= 0)
            {
                vpetDanceRecoverTimer = vpetDanceRecoverCD;
                health.VpetRecover(recoverPerDance);
            }
            //造成伤害
            if(vpetDanceAttackTimer <= 0)
            {
                bool isHurt = false;
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, damagePerDanceRadius, enemyLayer);

                foreach (Collider2D col in colliders)
                {
                    if (col != null)
                    {
                        col.gameObject.GetComponent<EnemyHealthSystem>().GetHurt(damagePerDance,transform.position,3f);   //造成伤害
                        isHurt = true;
                    }
                }
                if (isHurt)
                    vpetDanceAttackTimer = vpetDanceAttackCD;   //造成伤害则刷新CD

            }
        }
    }

    /// <summary>
    /// 等待跳舞持续阶段，淡出舞蹈音乐，再恢复行走、游戏音乐和进食权限并解除无敌。
    /// </summary>
    /// <returns>串联舞蹈等待、音乐淡出和收尾阶段的协程迭代器。</returns>
    IEnumerator DanceTime()
    {
        yield return new WaitForSeconds(13f);
        float duration = 2f;
        float elapsed = 0f;
        float startVolume = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentVolume = Mathf.Lerp(startVolume, 0f, t);
            AudioManager.Instance.AdjustBGMVolume(currentVolume);
            yield return null;
        }
        AudioManager.Instance.PauseOrContinueBGM(true); //暂停BGM
        currentState = VpetState.Walking;               //状态转变
        _animatorVpet.SetTrigger("DanceEnd");           //播放动画
        yield return new WaitForSeconds(0.5f);          //短暂等待
        AudioManager.Instance.AdjustBGMVolume(1);       //恢复BGM音源音量
        AudioManager.Instance.PlayBGM("GameMusic");     //播放游戏BGM
        isAllowEat = true;                              //允许进食
        isVpetDancing = false;                          //关闭跳舞状态
        health.isVpetInvincible = false;                //关闭无敌状态
    }

    #endregion

    #region 计时器更新

    /// <summary>
    /// 逐帧递减行为冷却计时器；仅在坠落确认启用时递减其计时器。
    /// </summary>
    private void TimerWork()
    {
        walkAudioTimer -= Time.deltaTime;
        climbAudioTimer -= Time.deltaTime;
        fallAudioTimer -= Time.deltaTime;
        sleepAudioTimer -= Time.deltaTime;
        vpetAttackTimer -= Time.deltaTime;
        sleepRecoverTimer -= Time.deltaTime;
        vpetDanceAttackTimer -= Time.deltaTime;
        vpetDanceRecoverTimer -= Time.deltaTime;
        if (isAllowFallCheckTimer) fallConfirmTimer -= Time.deltaTime;
    }

    #endregion

    #region 状态入口与结算

    /// <summary>
    /// 根据外部编号更新当前状态；不在此入口统一执行状态进入或退出清理。
    /// </summary>
    /// <param name="state">状态编号：0 待机、1 行走、2 飘飞、3 攀爬、4 进食、5 睡眠、6 跳舞、7 死亡、8 胜利；其他值仅输出日志。</param>
    public void VpetStateSet(int state)
    {
        switch (state)
        {
            case 0:
                currentState = VpetState.Idle;
                break;
            case 1:
                currentState = VpetState.Walking;
                break;
            case 2:
                currentState = VpetState.Fall;
                break;
            case 3:
                currentState = VpetState.Climb;
                break;
            case 4:
                currentState = VpetState.Eat;
                break;
            case 5:
                currentState = VpetState.Sleep;
                break;
            case 6:
                currentState = VpetState.Dance;
                break;
            case 7:
                currentState = VpetState.Die;
                break;
            case 8:
                currentState = VpetState.Win;
                break;
            default:
                Debug.Log("未知状态设置");
                break;
        }
    }

    /// <summary>
    /// 切换死亡状态，停止协程和飘飞表现，更新碰撞体并通知游戏管理器处理死亡。
    /// </summary>
    public void VpetDead()
    {
        currentState = VpetState.Die;   //更改状态
        StopAllCoroutines();            //停止其他所有协程
        VpetColliderChange();           //改变碰撞箱
        StopFallingLogic();             //进行一次坠落停止逻辑

        _animatorVpet.SetBool("isClimbing", false);         //停止攀爬状态
        AudioManager.Instance.PlaySound("die");             //播放死亡音效
        _animatorVpet.SetTrigger("Die");                    //设置动画
        GameManager.Instance.VpetDeadHandle();              //通知进行死亡处理

    }

    /// <summary>
    /// 切换胜利状态并停用常规行为更新，播放终点表现并通知游戏管理器处理胜利。
    /// </summary>
    public void VpetWin()
    {
        currentState = VpetState.Win;   //更改状态
        health.isVpetDead = true;       //防止执行其他操作
        StopAllCoroutines();            //停止其他所有协程
        VpetColliderChange();           //更新碰撞箱
        StopFallingLogic();             //进行一次坠落停止逻辑

        _animatorVpet.SetBool("isClimbing", false);         //停止攀爬状态
        AudioManager.Instance.PlaySound("win");             //播放到达终点音效
        AudioManager.Instance.PlaySound3D("setRespawnPoint", transform.position);   //音效播放
        Instantiate(winParticle, transform.position, Quaternion.identity);          //粒子效果
        _animatorVpet.SetTrigger("Win");                    //设置动画
        GameManager.Instance.VpetWinHandle();               //通知游戏管理器进行胜利处理

    }

    #endregion

    #region 碰撞体形状

    /// <summary>
    /// 为睡眠或死亡状态设置横向胶囊，其他状态恢复竖向胶囊。
    /// </summary>
    private void VpetColliderChange()
    {
        rigMotion.SetLyingCollider(currentState == VpetState.Sleep || currentState == VpetState.Die);
    }

    #endregion

    #region 提示文本

    [Tooltip("文本预制体")]
    [SerializeField] private GameObject TextPrefab;
    /// <summary>FigureCanvas父节点。</summary>
    private GameObject figureCanvas;
    /// <summary>
    /// 在提示画布下创建文本实例，并以浅黄色显示增益提示。
    /// </summary>
    /// <param name="text">要显示的提示文本。</param>
    private void ShowText(string text)
    {
        Transform parent = figureCanvas.transform;
        //创建TMP伤害数字实例
        GameObject figureText = Instantiate(TextPrefab, transform.position + Vector3.up, Quaternion.identity, parent);
        TextMeshProUGUI tmp = figureText.GetComponent<TextMeshProUGUI>();    //获取TMP

        tmp.SetText(text);
        tmp.color = new Color(1f, 1f, 0.4f, 1f);
    }

    #endregion

    #region 碰撞交互与攻击

    /// <summary>尖刺伤害。</summary>
    private float spikeDamage = 3f;
    /// <summary>
    /// 持续接触梯子时按当前状态进入攀爬，接触尖刺触发器时请求受伤。
    /// </summary>
    /// <param name="other">当前持续重叠的触发碰撞体。</param>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        //接触到梯子时
        if (other.CompareTag("Ladder"))
        {
            if (currentState == VpetState.Dance) return;
            if(currentState == VpetState.Idle || currentState == VpetState.Walking)
                currentState = VpetState.Climb;     //更改状态
            if(currentState == VpetState.Fall)
            {
                StopFallingLogic();                 //停止坠落
                isAllowEat = true;                  //允许进食
                currentState = VpetState.Climb;     //更改状态

            }
        }

        if (other.CompareTag("Spike"))
        {
            Vector2 dir = transform.position.y > other.transform.position.y ? Vector2.up : Vector2.down;
            health.VpetGethurt(spikeDamage, dir * 165f);
        }

    }

    /// <summary>
    /// 攀爬期间离开梯子时施加向上推力，并切换回行走状态。
    /// </summary>
    /// <param name="other">刚刚结束重叠的触发碰撞体。</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        //离开梯子时
        if (other.CompareTag("Ladder") && isClimbing)
        {
            rigMotion.PushOffLadder();
            currentState = VpetState.Walking;     //更改状态
        }
    }

    /// <summary>桌宠攻击伤害。</summary>
    private float vpetAttackDamage = 3f;
    /// <summary>桌宠攻击计时器。</summary>
    private float vpetAttackTimer;
    /// <summary>普通攻击的基础冷却间隔，单位为秒。</summary>
    private float vpetAttackCD = 1.5f;

    /// <summary>攻击Buff伤害修正。</summary>
    private float attackBuffDamageFix = 1f;
    /// <summary>普通攻击冷却间隔的修正倍率。</summary>
    private float attackBuffTimeFix = 1f;

    /// <summary>是否一拳。</summary>
    private bool isOnePunch = false;
    /// <summary>
    /// 处理实体尖刺伤害，并在行走状态及攻击冷却允许时攻击接触的敌人。
    /// </summary>
    /// <param name="other">当前持续接触的碰撞信息，包含对方碰撞体与对象。</param>
    private void OnCollisionStay2D(Collision2D other)
    {
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        //若碰到尖刺
        if (other.collider.CompareTag("Spike"))
        {
            Vector2 dir = transform.position.y > other.transform.position.y ? Vector2.up + Vector2.right * 0.5f : Vector2.down;
            health.VpetGethurt(spikeDamage, dir * 165f);
        }

        //若碰到敌人
        if (other.collider.CompareTag("Enemy") && vpetAttackTimer <= 0 && currentState == VpetState.Walking)
        {
            vpetAttackTimer = vpetAttackCD * attackBuffTimeFix;     //攻击CD重置
            var enemyHealth = other.gameObject.GetComponent<EnemyHealthSystem>();

            if (enemyHealth != null)
            {
                //若处于肾宝状态
                if (isOnePunch)
                {
                    isOnePunch = false;
                    onePunchState.SetActive(false);  //关闭Effect状态图
                    AudioManager.Instance.PlaySound("OnePunch");
                    enemyHealth.GetHurt(vpetAttackDamage * 999 * attackBuffDamageFix, transform.position,25f);
                    CameraShake.Instance.ShakeScreen();
                }
                else
                    enemyHealth.GetHurt(vpetAttackDamage * attackBuffDamageFix, transform.position,5f);

            }
        }

    }
    #endregion
}

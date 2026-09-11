using UnityEngine;

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

    [Tooltip("敌人层")]
    [SerializeField] private LayerMask enemyLayer;

    /// <summary>负责地面接触、射线与瞬移重叠查询的协作对象。</summary>
    private VpetEnvironmentSensor environmentSensor;

    /// <summary>负责施力、位置和碰撞体操作的协作对象。</summary>
    private VpetRigMotion rigMotion;

    /// <summary>负责普通攻击、跳舞攻击与攻击冷却的协作对象。</summary>
    private VpetAttack attack;

    /// <summary>负责限时增益数值状态的协作对象。</summary>
    private VpetEffect effect;

    /// <summary>桌宠生命系统。</summary>
    private VpetHealthSystem health;

    /// <summary>负责状态注册、当前状态维护和状态进入退出回调的状态机。</summary>
    private VpetStateMachine stateMachine;

    /// <summary>负责飘飞行为及落地起身流程的状态处理器。</summary>
    private VpetState_Fall fallState;

    /// <summary>负责睡眠协程的状态处理器。</summary>
    private VpetState_Sleep sleepState;

    /// <summary>负责进食和食物效果分派的状态处理器。</summary>
    private VpetState_Eat eatState;

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

        // 协作对象只执行显式调用，不引入额外的 Unity 生命周期顺序。
        environmentSensor = new VpetEnvironmentSensor(body, capsule);
        rigMotion = new VpetRigMotion(body, floatingForce, capsule);
        attack = new VpetAttack();
        effect = new VpetEffect();
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");

        // 先创建状态对象，再创建状态机；回调捕获的状态机引用会在下一步完成赋值。
        fallState = new VpetState_Fall(
            _animatorVpet,
            environmentSensor,
            rigMotion,
            effect,
            this,
            nextState => stateMachine.SetState(nextState),
            allow => isAllowEat = allow);
        sleepState = new VpetState_Sleep(
            this,
            _animatorVpet,
            health,
            rigMotion,
            nextState => stateMachine.SetState(nextState),
            allow => isAllowEat = allow);
        eatState = new VpetState_Eat(
            this,
            _animatorVpet,
            _animatorVpetHand,
            _animatorEatenItem,
            eatenItemSprite,
            health,
            attack,
            effect,
            environmentSensor,
            rigMotion,
            transform,
            nextState => stateMachine.SetState(nextState),
            () =>
            {
                if (fallState.IsFalling)
                    fallState.StopFallingLogic();
            },
            () => sleepState.BeginSleep(),
            () => isAllowEat,
            allow => isAllowEat = allow,
            TextPrefab,
            figureCanvas,
            speedUpParticle,
            attackUpParticle,
            onePunchState,
            onePunchEffect,
            bombPrefab);

        VpetState_Dance danceState = new VpetState_Dance(
            this,
            _animatorVpet,
            health,
            attack,
            transform,
            nextState => stateMachine.SetState(nextState),
            allow => isAllowEat = allow,
            enemyLayer);
        VpetState_Die dieState = new VpetState_Die(
            this,
            rigMotion,
            _animatorVpet,
            () => fallState.StopFallingLogic(),
            () => GameManager.Instance.VpetDeadHandle());
        VpetState_Win winState = new VpetState_Win(
            this,
            rigMotion,
            _animatorVpet,
            health,
            () => fallState.StopFallingLogic(),
            transform,
            winParticle,
            () => GameManager.Instance.VpetWinHandle());

        stateMachine = new VpetStateMachine(
            VpetState.Idle,
            new VpetState_Walking(_animatorVpet, environmentSensor, rigMotion, effect),
            new VpetState_Climb(_animatorVpet, rigMotion),
            fallState,
            eatState,
            sleepState,
            danceState,
            dieState,
            winState);
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
        stateMachine.FixedUpdate(); //由当前状态执行物理帧行为
    }

    /// <summary>
    /// 逐帧更新飘飞流程、地面探测、跳舞效果和各行为计时器。
    /// </summary>
    private void Update()
    {
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        VpetFallCheck();    //桌宠飘飞行为监测
        stateMachine.Update(Time.deltaTime); //由当前状态执行普通帧行为
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
                attack.ConfigureBaseValues(4f, 1.2f);
                spikeDamage = 2f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                attack.ConfigureBaseValues(3f, 1.3f);
                spikeDamage = 3f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                attack.ConfigureBaseValues(2f, 1.4f);
                spikeDamage = 4f;
                break;
        }
    }

    #endregion

    #region 地面接触配置

    [Tooltip("仅地面层级")]
    [SerializeField] LayerMask groundLayer;

    [Tooltip("所有地面有关层级")]
    [SerializeField] LayerMask allGroundLayer;

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
        eatState.TryEat(item);
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
        if (stateMachine.CurrentState == VpetState.Walking || stateMachine.CurrentState == VpetState.Idle)
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
                stateMachine.SetState(VpetState.Fall);      //确认转换为飘飞状态
                isAllowFallCheckTimer = false;      //停止计时器使用
            }
            else
                isAllowFallCheckTimer = false;      //停止计时器使用
        }
    }

    #endregion


    #region 计时器更新

    /// <summary>
    /// 逐帧递减行为冷却计时器；仅在坠落确认启用时递减其计时器。
    /// </summary>
    private void TimerWork()
    {
        attack.Tick(Time.deltaTime);
        if (isAllowFallCheckTimer) fallConfirmTimer -= Time.deltaTime;
    }

    #endregion

    #region 状态入口与结算

    /// <summary>
    /// 根据外部编号请求状态机切换当前状态；具体状态生命周期由状态机处理。
    /// </summary>
    /// <param name="state">状态编号：0 待机、1 行走、2 飘飞、3 攀爬、4 进食、5 睡眠、6 跳舞、7 死亡、8 胜利；其他值仅输出日志。</param>
    public void VpetStateSet(int state)
    {
        switch (state)
        {
            case 0:
                stateMachine.SetState(VpetState.Idle);
                break;
            case 1:
                stateMachine.SetState(VpetState.Walking);
                break;
            case 2:
                stateMachine.SetState(VpetState.Fall);
                break;
            case 3:
                stateMachine.SetState(VpetState.Climb);
                break;
            case 4:
                stateMachine.SetState(VpetState.Eat);
                break;
            case 5:
                stateMachine.SetState(VpetState.Sleep);
                break;
            case 6:
                stateMachine.SetState(VpetState.Dance);
                break;
            case 7:
                stateMachine.SetState(VpetState.Die);
                break;
            case 8:
                stateMachine.SetState(VpetState.Win);
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
        stateMachine.SetState(VpetState.Die);
    }

    /// <summary>
    /// 切换胜利状态并停用常规行为更新，播放终点表现并通知游戏管理器处理胜利。
    /// </summary>
    public void VpetWin()
    {
        stateMachine.SetState(VpetState.Win);
    }

    #endregion

    #region 碰撞体形状

    /// <summary>
    /// 为睡眠或死亡状态设置横向胶囊，其他状态恢复竖向胶囊。
    /// </summary>
    private void VpetColliderChange()
    {
        rigMotion.SetLyingCollider(stateMachine.CurrentState == VpetState.Sleep || stateMachine.CurrentState == VpetState.Die);
    }

    #endregion

    #region 提示文本

    [Tooltip("文本预制体")]
    [SerializeField] private GameObject TextPrefab;
    /// <summary>FigureCanvas父节点。</summary>
    private GameObject figureCanvas;

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
            if (stateMachine.CurrentState == VpetState.Dance) return;
            if(stateMachine.CurrentState == VpetState.Idle || stateMachine.CurrentState == VpetState.Walking)
                stateMachine.SetState(VpetState.Climb);     //更改状态
            if(stateMachine.CurrentState == VpetState.Fall)
            {
                fallState.StopFallingLogic();       //停止坠落
                isAllowEat = true;                  //允许进食
                stateMachine.SetState(VpetState.Climb);     //更改状态

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
        if (other.CompareTag("Ladder") && stateMachine.CurrentState == VpetState.Climb)
        {
            rigMotion.PushOffLadder();
            stateMachine.SetState(VpetState.Walking);     //更改状态
        }
    }

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
        if (other.collider.CompareTag("Enemy") && attack.CanStartNormalAttack() && stateMachine.CurrentState == VpetState.Walking)
        {
            attack.StartNormalAttackCooldown();     //攻击CD重置
            var enemyHealth = other.gameObject.GetComponent<EnemyHealthSystem>();

            if (enemyHealth != null)
            {
                // 一拳状态只在实际找到敌人生命系统后消耗，保持旧调用顺序。
                if (attack.ApplyNormalAttack(enemyHealth, transform.position))
                {
                    onePunchState.SetActive(false);  //关闭Effect状态图
                    AudioManager.Instance.PlaySound("OnePunch");
                    CameraShake.Instance.ShakeScreen();
                }
            }
        }

    }
    #endregion
}

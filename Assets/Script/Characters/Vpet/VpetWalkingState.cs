using UnityEngine;

/// <summary>
/// 桌宠行走状态
/// - 作为状态机中的行走状态处理器，执行行走表现、环境读取和刚体施力。
/// </summary>
public sealed class VpetWalkingState : VpetStateBase
{
    /// <summary>负责读取地面与水面信息的环境探测器。</summary>
    private readonly VpetEnvironmentSensor environmentSensor;

    /// <summary>负责沿接触面施加行走力的刚体运动对象。</summary>
    private readonly VpetRigMotion rigMotion;

    /// <summary>负责读取行走增益倍率的效果对象。</summary>
    private readonly VpetEffect effect;

    /// <summary>负责同步行走动画的桌宠动画器。</summary>
    private readonly Animator animator;

    /// <summary>行走或游泳音效距离下次播放的剩余时间，单位为秒。</summary>
    private float walkAudioTimer;

    /// <summary>行走或游泳音效的播放间隔，单位为秒。</summary>
    private const float WalkAudioCooldown = 0.62f;

    #region 初始化与状态行为

    /// <summary>创建不绑定外部依赖的行走状态占位处理器。</summary>
    public VpetWalkingState() : this(null, null, null, null)
    {
    }

    /// <summary>
    /// 创建行走状态处理器并注入行走所需依赖。
    /// </summary>
    /// <param name="animator">用于播放行走表现的桌宠动画器。</param>
    /// <param name="environmentSensor">用于读取地面与水面信息的环境探测器。</param>
    /// <param name="rigMotion">用于执行 Rigidbody2D 行走施力的运动对象。</param>
    /// <param name="effect">用于读取当前速度增益倍率的效果对象。</param>
    public VpetWalkingState(
        Animator animator,
        VpetEnvironmentSensor environmentSensor,
        VpetRigMotion rigMotion,
        VpetEffect effect) : base(VpetState.Walking)
    {
        this.animator = animator;
        this.environmentSensor = environmentSensor;
        this.rigMotion = rigMotion;
        this.effect = effect;
    }

    /// <summary>在行走状态的普通帧递减行走音效计时器。</summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    public override void OnUpdate(float deltaTime)
    {
        walkAudioTimer -= deltaTime;
    }

    /// <summary>
    /// 在物理帧内同步行走动画、播放间隔音效，并沿接触面施加行走力。
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (animator == null || environmentSensor == null || rigMotion == null || effect == null)
            return;

        if (walkAudioTimer <= 0f)
        {
            walkAudioTimer = WalkAudioCooldown;
            AudioManager.Instance.PlaySound(environmentSensor.IsInWater ? "swim" : "walk");
        }

        animator.SetBool("isWalk", true);
        rigMotion.Walk(
            environmentSensor.GetAverageGroundNormal(),
            environmentSensor.IsInWater,
            environmentSensor.IsInWater ? false : environmentSensor.HasGroundContact(),
            environmentSensor.IsGrounded,
            effect.SpeedForceMultiplier);
    }

    /// <summary>离开行走状态时关闭行走动画参数。</summary>
    public override void OnExit()
    {
        if (animator != null)
            animator.SetBool("isWalk", false);
    }

    #endregion
}

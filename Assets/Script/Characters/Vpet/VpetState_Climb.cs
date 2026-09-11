using UnityEngine;

/// <summary>
/// 桌宠攀爬状态
/// - 执行攀爬表现、攀爬音效和 Rigidbody2D 向上驱动。
/// </summary>
public sealed class VpetState_Climb : VpetStateBase
{
    /// <summary>负责同步攀爬动画的桌宠动画器。</summary>
    private readonly Animator animator;

    /// <summary>负责执行攀爬速度控制的刚体运动对象。</summary>
    private readonly VpetRigMotion rigMotion;

    /// <summary>攀爬音效距离下次播放的剩余时间，单位为秒。</summary>
    private float climbAudioTimer;

    /// <summary>攀爬音效的播放间隔，单位为秒。</summary>
    private const float ClimbAudioCooldown = 0.62f;

    #region 初始化与状态行为

    /// <summary>创建不绑定外部依赖的攀爬状态占位处理器。</summary>
    public VpetState_Climb() : this(null, null)
    {
    }

    /// <summary>
    /// 创建攀爬状态处理器并注入动画器与刚体运动对象。
    /// </summary>
    /// <param name="animator">用于播放攀爬表现的桌宠动画器。</param>
    /// <param name="rigMotion">用于执行攀爬速度控制的刚体运动对象。</param>
    public VpetState_Climb(Animator animator, VpetRigMotion rigMotion) : base(VpetState.Climb)
    {
        this.animator = animator;
        this.rigMotion = rigMotion;
    }

    /// <summary>在攀爬状态的普通帧递减攀爬音效计时器。</summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    public override void OnUpdate(float deltaTime)
    {
        climbAudioTimer -= deltaTime;
    }

    /// <summary>
    /// 在物理帧内启动攀爬动画、播放间隔音效并持续施加向上攀爬力。
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (animator == null || rigMotion == null)
            return;

        animator.ResetTrigger("ClimbEnd");
        animator.SetBool("isClimbing", true);

        if (climbAudioTimer <= 0f)
        {
            climbAudioTimer = ClimbAudioCooldown;
            int randomIndex = Random.Range(1, 5);
            AudioManager.Instance.PlaySound($"Assets/Audio/Vpet/climb/ladder{randomIndex}.wav");
        }

        rigMotion.Climb();
    }

    /// <summary>进入攀爬状态时清零刚体速度并播放攀爬开始动画。</summary>
    public override void OnEnter()
    {
        if (animator == null || rigMotion == null)
            return;

        rigMotion.ResetVelocity();
        animator.SetTrigger("ClimbStart");
    }

    /// <summary>离开攀爬状态时关闭攀爬动画参数并播放结束动画。</summary>
    public override void OnExit()
    {
        if (animator == null)
            return;

        animator.SetBool("isClimbing", false);
        animator.SetTrigger("ClimbEnd");
    }

    #endregion
}

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 桌宠飘飞状态
/// - 管理飘飞开始、水平修正、落地检测和起身协程。
/// </summary>
public sealed class VpetState_Fall : VpetStateBase
{
    /// <summary>负责读取地面探测结果的环境探测器。</summary>
    private readonly VpetEnvironmentSensor environmentSensor;

    /// <summary>负责持续升力、水平驱动和速度操作的刚体运动对象。</summary>
    private readonly VpetRigMotion rigMotion;

    /// <summary>用于读取飘飞期间速度增益倍率的效果对象。</summary>
    private readonly VpetEffect effect;

    /// <summary>负责播放飘飞表现的桌宠动画器。</summary>
    private readonly Animator animator;

    /// <summary>负责运行起身协程的 Unity 宿主。</summary>
    private readonly MonoBehaviour coroutineRunner;

    /// <summary>请求状态机切换到目标状态。</summary>
    private readonly Action<VpetState> requestState;

    /// <summary>修改桌宠进食权限的回调。</summary>
    private readonly Action<bool> setAllowEat;

    /// <summary>是否正在执行飘飞阶段。</summary>
    public bool IsFalling { get; private set; }

    /// <summary>飘飞音效距离下次播放的剩余时间，单位为秒。</summary>
    private float fallAudioTimer;

    /// <summary>飘飞音效的播放间隔，单位为秒。</summary>
    private const float FallAudioCooldown = 1f;

    /// <summary>起身协程是否正在等待，用于防止重复启动。</summary>
    private bool isGetUpCoroutineWork;

    #region 初始化与状态行为

    /// <summary>创建不绑定外部依赖的飘飞状态占位处理器。</summary>
    public VpetState_Fall() : this(null, null, null, null, null, null, null)
    {
    }

    /// <summary>
    /// 创建飘飞状态处理器并注入飘飞流程所需依赖。
    /// </summary>
    /// <param name="animator">用于播放飘飞表现的桌宠动画器。</param>
    /// <param name="environmentSensor">用于读取地面探测结果的环境探测器。</param>
    /// <param name="rigMotion">用于执行飘飞物理操作的刚体运动对象。</param>
    /// <param name="effect">用于读取当前速度增益倍率的效果对象。</param>
    /// <param name="coroutineRunner">用于启动起身协程的 Unity 宿主。</param>
    /// <param name="requestState">请求状态机切换状态的回调。</param>
    /// <param name="setAllowEat">修改桌宠进食权限的回调。</param>
    public VpetState_Fall(
        Animator animator,
        VpetEnvironmentSensor environmentSensor,
        VpetRigMotion rigMotion,
        VpetEffect effect,
        MonoBehaviour coroutineRunner,
        Action<VpetState> requestState,
        Action<bool> setAllowEat) : base(VpetState.Fall)
    {
        this.animator = animator;
        this.environmentSensor = environmentSensor;
        this.rigMotion = rigMotion;
        this.effect = effect;
        this.coroutineRunner = coroutineRunner;
        this.requestState = requestState;
        this.setAllowEat = setAllowEat;
    }

    /// <summary>
    /// 处理飘飞开始、循环音效和落地起身流程。
    /// </summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    public override void OnUpdate(float deltaTime)
    {
        if (animator == null || environmentSensor == null || rigMotion == null || coroutineRunner == null)
            return;

        fallAudioTimer -= deltaTime;

        if (!environmentSensor.IsGrounded && !isGetUpCoroutineWork)
        {
            if (!IsFalling)
            {
                IsFalling = true;
                animator.SetBool("isFalling", true);
                animator.SetTrigger("FallStart");
                AudioManager.Instance.PlaySound("startFall");
                rigMotion.StartFloating();
            }

            if (fallAudioTimer <= 0f)
            {
                fallAudioTimer = FallAudioCooldown;
                AudioManager.Instance.PlaySound("fall");
            }
        }

        if (environmentSensor.IsGrounded && IsFalling && !isGetUpCoroutineWork)
        {
            setAllowEat(false);
            StopFallingLogic();
            coroutineRunner.StartCoroutine(VpetGetUp());
            AudioManager.Instance.PlaySound("fallen");
        }
    }

    /// <summary>在物理帧内修正飘飞期间的水平速度。</summary>
    public override void OnFixedUpdate()
    {
        if (environmentSensor == null || rigMotion == null || effect == null)
            return;

        if (IsFalling && !environmentSensor.IsGrounded && !isGetUpCoroutineWork)
            rigMotion.DriveFloatingHorizontal(effect.SpeedForceMultiplier);
    }

    /// <summary>
    /// 清除飘飞标记、持续力、动画和循环音效，供其他状态入口中断飘飞流程。
    /// </summary>
    public void StopFallingLogic()
    {
        IsFalling = false;
        if (animator != null)
            animator.SetBool("isFalling", false);
        rigMotion?.StopFloating();
        AudioManager.Instance.StopSound("fall");
    }

    /// <summary>离开飘飞状态时确保持续升力和飘飞表现被关闭。</summary>
    public override void OnExit()
    {
        if (IsFalling)
            StopFallingLogic();
    }

    /// <summary>
    /// 等待落地起身阶段结束，再恢复行走状态和进食权限。
    /// </summary>
    /// <returns>控制起身等待及流程标记的协程迭代器。</returns>
    private IEnumerator VpetGetUp()
    {
        isGetUpCoroutineWork = true;
        yield return new WaitForSeconds(2.5f);
        setAllowEat(true);
        requestState(VpetState.Walking);
        isGetUpCoroutineWork = false;
    }

    #endregion
}

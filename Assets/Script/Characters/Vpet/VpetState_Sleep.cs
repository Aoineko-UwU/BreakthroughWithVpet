using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 桌宠睡眠状态
/// - 管理入睡表现、睡眠期间回血和起身收尾。
/// </summary>
public sealed class VpetState_Sleep : VpetStateBase
{
    /// <summary>负责运行睡眠协程的 Unity 宿主。</summary>
    private readonly MonoBehaviour coroutineRunner;

    /// <summary>负责播放睡眠动画的桌宠动画器。</summary>
    private readonly Animator animator;

    /// <summary>负责睡眠期间生命恢复的生命系统。</summary>
    private readonly VpetHealthSystem health;

    /// <summary>负责睡眠碰撞体切换的刚体运动对象。</summary>
    private readonly VpetRigMotion rigMotion;

    /// <summary>睡眠完成后的状态切换请求。</summary>
    private readonly Action<VpetState> requestState;

    /// <summary>修改桌宠进食权限的回调。</summary>
    private readonly Action<bool> setAllowEat;

    /// <summary>睡眠持续时间，不含入睡和起身动画等待。</summary>
    private const float SleepDuration = 7.5f;

    /// <summary>睡眠循环音效播放间隔。</summary>
    private const float SleepAudioCooldown = 2.3f;

    /// <summary>睡眠回血间隔。</summary>
    private const float SleepRecoverCooldown = 1f;

    /// <summary>每次睡眠回血量。</summary>
    private const float SleepRecoverRate = 1f;

    /// <summary>睡眠音效计时器。</summary>
    private float sleepAudioTimer;

    /// <summary>睡眠回血计时器。</summary>
    private float sleepRecoverTimer;

    #region 初始化与状态行为

    /// <summary>创建不绑定外部依赖的睡眠状态占位处理器。</summary>
    public VpetState_Sleep() : base(VpetState.Sleep)
    {
    }

    /// <summary>
    /// 创建睡眠状态处理器并注入睡眠流程所需依赖。
    /// </summary>
    /// <param name="coroutineRunner">用于运行睡眠协程的 Unity 宿主。</param>
    /// <param name="animator">用于播放睡眠动画的桌宠动画器。</param>
    /// <param name="health">用于执行睡眠回血的生命系统。</param>
    /// <param name="rigMotion">用于切换睡眠碰撞体的刚体运动对象。</param>
    /// <param name="requestState">请求状态机切换状态的回调。</param>
    /// <param name="setAllowEat">修改桌宠进食权限的回调。</param>
    public VpetState_Sleep(
        MonoBehaviour coroutineRunner,
        Animator animator,
        VpetHealthSystem health,
        VpetRigMotion rigMotion,
        Action<VpetState> requestState,
        Action<bool> setAllowEat) : base(VpetState.Sleep)
    {
        this.coroutineRunner = coroutineRunner;
        this.animator = animator;
        this.health = health;
        this.rigMotion = rigMotion;
        this.requestState = requestState;
        this.setAllowEat = setAllowEat;
    }

    /// <summary>启动完整的入睡、睡眠和起身协程。</summary>
    /// <returns>控制睡眠流程的协程对象。</returns>
    public Coroutine BeginSleep()
    {
        return coroutineRunner.StartCoroutine(SleepRoutine());
    }

    /// <summary>睡眠阶段由协程驱动，不在状态机普通帧中重复执行。</summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    public override void OnUpdate(float deltaTime)
    {
    }

    /// <summary>睡眠阶段不额外执行物理帧行为。</summary>
    public override void OnFixedUpdate()
    {
    }

    #endregion

    #region 睡眠协程

    /// <summary>
    /// 播放入睡和起身动画，在睡眠期间定时恢复生命，结束后恢复行走及进食权限。
    /// </summary>
    /// <returns>串联入睡、睡眠和起身阶段的协程迭代器。</returns>
    private IEnumerator SleepRoutine()
    {
        requestState(VpetState.Sleep);
        AudioManager.Instance.PlaySound("startSleep");
        animator.SetTrigger("SleepStart");
        yield return new WaitForSeconds(2.5f);

        rigMotion.SetLyingCollider(true);
        float elapsedTime = 0f;
        while (elapsedTime < SleepDuration)
        {
            elapsedTime += Time.deltaTime;
            sleepAudioTimer -= Time.deltaTime;
            sleepRecoverTimer -= Time.deltaTime;

            if (sleepAudioTimer <= 0f)
            {
                sleepAudioTimer = SleepAudioCooldown;
                AudioManager.Instance.PlaySound("sleeping");
            }

            if (sleepRecoverTimer <= 0f)
            {
                sleepRecoverTimer = SleepRecoverCooldown;
                health.VpetRecover(SleepRecoverRate);
            }

            yield return null;
        }

        AudioManager.Instance.StopSound("sleeping");
        animator.SetTrigger("SleepEnd");
        yield return new WaitForSeconds(0.9f);
        requestState(VpetState.Walking);
        rigMotion.SetLyingCollider(false);
        setAllowEat(true);
    }

    #endregion
}

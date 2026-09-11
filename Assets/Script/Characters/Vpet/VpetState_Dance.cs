using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 桌宠跳舞状态
/// - 管理跳舞表现、无敌、持续回血、范围攻击和音乐收尾。
/// </summary>
public sealed class VpetState_Dance : VpetStateBase
{
    /// <summary>负责运行跳舞收尾协程的 Unity 宿主。</summary>
    private readonly MonoBehaviour coroutineRunner;

    /// <summary>负责播放跳舞表现的桌宠动画器。</summary>
    private readonly Animator animator;

    /// <summary>负责跳舞期间无敌和回血的生命系统。</summary>
    private readonly VpetHealthSystem health;

    /// <summary>负责跳舞范围攻击的攻击对象。</summary>
    private readonly VpetAttack attack;

    /// <summary>用于计算范围攻击中心点的桌宠变换。</summary>
    private readonly Transform ownerTransform;

    /// <summary>跳舞结束后的状态切换请求。</summary>
    private readonly Action<VpetState> requestState;

    /// <summary>修改桌宠进食权限的回调。</summary>
    private readonly Action<bool> setAllowEat;

    /// <summary>敌人所在层级。</summary>
    private readonly LayerMask enemyLayer;

    /// <summary>是否已经启动跳舞表现和收尾协程。</summary>
    private bool isDancing;

    /// <summary>跳舞生命恢复计时器。</summary>
    private float recoverTimer;

    /// <summary>跳舞生命恢复间隔。</summary>
    private const float RecoverCooldown = 1f;

    /// <summary>每次跳舞恢复的生命值。</summary>
    private const float RecoverRate = 1f;

    #region 初始化与状态行为

    /// <summary>创建不绑定外部依赖的跳舞状态占位处理器。</summary>
    public VpetState_Dance() : base(VpetState.Dance)
    {
    }

    /// <summary>
    /// 创建跳舞状态处理器并注入跳舞流程所需依赖。
    /// </summary>
    /// <param name="coroutineRunner">用于运行跳舞收尾协程的 Unity 宿主。</param>
    /// <param name="animator">用于播放跳舞动画的桌宠动画器。</param>
    /// <param name="health">用于处理无敌和回血的生命系统。</param>
    /// <param name="attack">用于执行范围攻击的攻击对象。</param>
    /// <param name="ownerTransform">桌宠主体的世界变换。</param>
    /// <param name="requestState">请求状态机切换状态的回调。</param>
    /// <param name="setAllowEat">修改桌宠进食权限的回调。</param>
    /// <param name="enemyLayer">敌人所在层级。</param>
    public VpetState_Dance(
        MonoBehaviour coroutineRunner,
        Animator animator,
        VpetHealthSystem health,
        VpetAttack attack,
        Transform ownerTransform,
        Action<VpetState> requestState,
        Action<bool> setAllowEat,
        LayerMask enemyLayer) : base(VpetState.Dance)
    {
        this.coroutineRunner = coroutineRunner;
        this.animator = animator;
        this.health = health;
        this.attack = attack;
        this.ownerTransform = ownerTransform;
        this.requestState = requestState;
        this.setAllowEat = setAllowEat;
        this.enemyLayer = enemyLayer;
    }

    /// <summary>
    /// 在普通帧内启用无敌、定时回血并执行跳舞范围攻击。
    /// </summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    public override void OnUpdate(float deltaTime)
    {
        if (coroutineRunner == null || animator == null || health == null || attack == null)
            return;

        setAllowEat(false);
        health.isVpetInvincible = true;

        if (!isDancing)
        {
            isDancing = true;
            animator.SetTrigger("Dance");
            AudioManager.Instance.PlayBGM("DanceMusic");
            coroutineRunner.StartCoroutine(DanceRoutine());
        }

        if (recoverTimer <= 0f)
        {
            recoverTimer = RecoverCooldown;
            health.VpetRecover(RecoverRate);
        }

        attack.TryPerformDanceAttack(ownerTransform.position, enemyLayer);
        recoverTimer -= deltaTime;
    }

    /// <summary>跳舞状态不额外执行物理帧行为。</summary>
    public override void OnFixedUpdate()
    {
    }

    #endregion

    #region 跳舞收尾

    /// <summary>
    /// 等待跳舞持续阶段，淡出舞蹈音乐，再恢复行走、游戏音乐和进食权限并解除无敌。
    /// </summary>
    /// <returns>串联舞蹈等待、音乐淡出和收尾阶段的协程迭代器。</returns>
    private IEnumerator DanceRoutine()
    {
        yield return new WaitForSeconds(13f);
        float duration = 2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            AudioManager.Instance.AdjustBGMVolume(Mathf.Lerp(1f, 0f, elapsed / duration));
            yield return null;
        }

        AudioManager.Instance.PauseOrContinueBGM(true);
        requestState(VpetState.Walking);
        animator.SetTrigger("DanceEnd");
        yield return new WaitForSeconds(0.5f);
        AudioManager.Instance.AdjustBGMVolume(1f);
        AudioManager.Instance.PlayBGM("GameMusic");
        setAllowEat(true);
        isDancing = false;
        health.isVpetInvincible = false;
    }

    #endregion
}

using System;
using UnityEngine;

/// <summary>
/// 桌宠死亡状态
/// - 负责停止运行流程、切换死亡碰撞体、播放死亡表现并通知游戏管理器。
/// </summary>
public sealed class VpetState_Die : VpetStateBase
{
    /// <summary>用于停止桌宠协程的 Unity 宿主。</summary>
    private readonly MonoBehaviour coroutineRunner;

    /// <summary>负责切换死亡碰撞体的刚体运动对象。</summary>
    private readonly VpetRigMotion rigMotion;

    /// <summary>负责播放死亡表现的桌宠动画器。</summary>
    private readonly Animator animator;

    /// <summary>停止飘飞流程的回调。</summary>
    private readonly Action stopFalling;

    /// <summary>通知游戏管理器处理死亡的回调。</summary>
    private readonly Action notifyDead;

    #region 初始化与状态行为

    /// <summary>创建不绑定外部依赖的死亡状态占位处理器。</summary>
    public VpetState_Die() : base(VpetState.Die)
    {
    }

    /// <summary>
    /// 创建死亡状态处理器并注入死亡结算所需依赖。
    /// </summary>
    /// <param name="coroutineRunner">用于停止桌宠协程的 Unity 宿主。</param>
    /// <param name="rigMotion">用于切换死亡碰撞体的刚体运动对象。</param>
    /// <param name="animator">用于播放死亡表现的桌宠动画器。</param>
    /// <param name="stopFalling">停止飘飞流程的回调。</param>
    /// <param name="notifyDead">通知游戏管理器处理死亡的回调。</param>
    public VpetState_Die(
        MonoBehaviour coroutineRunner,
        VpetRigMotion rigMotion,
        Animator animator,
        Action stopFalling,
        Action notifyDead) : base(VpetState.Die)
    {
        this.coroutineRunner = coroutineRunner;
        this.rigMotion = rigMotion;
        this.animator = animator;
        this.stopFalling = stopFalling;
        this.notifyDead = notifyDead;
    }

    /// <summary>进入死亡状态时停止协程、关闭飘飞和攀爬表现并播放死亡结算。</summary>
    public override void OnEnter()
    {
        coroutineRunner?.StopAllCoroutines();
        stopFalling?.Invoke();
        rigMotion?.SetLyingCollider(true);
        if (animator != null)
        {
            animator.SetBool("isClimbing", false);
            animator.SetTrigger("Die");
        }
        AudioManager.Instance.PlaySound("die");
        notifyDead?.Invoke();
    }

    /// <summary>死亡状态不重复执行普通帧行为。</summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    public override void OnUpdate(float deltaTime)
    {
    }

    /// <summary>死亡状态不执行物理帧行为。</summary>
    public override void OnFixedUpdate()
    {
    }

    #endregion
}

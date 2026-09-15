using System;
using UnityEngine;

/// <summary>
/// 桌宠胜利状态
/// - 负责停止常规行为、播放终点表现并通知游戏管理器处理胜利。
/// </summary>
public sealed class VpetState_Win : VpetStateBase
{
    /// <summary>用于停止桌宠协程的 Unity 宿主。</summary>
    private readonly MonoBehaviour coroutineRunner;

    /// <summary>负责切换胜利阶段碰撞体的刚体运动对象。</summary>
    private readonly VpetRigMotion rigMotion;

    /// <summary>负责播放胜利表现的桌宠动画器。</summary>
    private readonly Animator animator;

    /// <summary>用于标记桌宠结算结束的生命系统。</summary>
    private readonly VpetHealthSystem health;

    /// <summary>停止飘飞流程的回调。</summary>
    private readonly Action stopFalling;

    /// <summary>桌宠主体的世界变换。</summary>
    private readonly Transform ownerTransform;

    /// <summary>胜利粒子预制体。</summary>
    private readonly GameObject winParticle;

    /// <summary>通知游戏管理器处理胜利的回调。</summary>
    private readonly Action notifyWin;

    #region 初始化与状态行为

    /// <summary>创建不绑定外部依赖的胜利状态占位处理器。</summary>
    public VpetState_Win() : base(VpetState.Win)
    {
    }

    /// <summary>
    /// 创建胜利状态处理器并注入胜利结算所需依赖。
    /// </summary>
    /// <param name="coroutineRunner">用于停止桌宠协程的 Unity 宿主。</param>
    /// <param name="rigMotion">用于切换胜利阶段碰撞体的刚体运动对象。</param>
    /// <param name="animator">用于播放胜利表现的桌宠动画器。</param>
    /// <param name="health">用于标记桌宠结算结束的生命系统。</param>
    /// <param name="stopFalling">停止飘飞流程的回调。</param>
    /// <param name="ownerTransform">桌宠主体的世界变换。</param>
    /// <param name="winParticle">胜利粒子预制体。</param>
    /// <param name="notifyWin">通知游戏管理器处理胜利的回调。</param>
    public VpetState_Win(
        MonoBehaviour coroutineRunner,
        VpetRigMotion rigMotion,
        Animator animator,
        VpetHealthSystem health,
        Action stopFalling,
        Transform ownerTransform,
        GameObject winParticle,
        Action notifyWin) : base(VpetState.Win)
    {
        this.coroutineRunner = coroutineRunner;
        this.rigMotion = rigMotion;
        this.animator = animator;
        this.health = health;
        this.stopFalling = stopFalling;
        this.ownerTransform = ownerTransform;
        this.winParticle = winParticle;
        this.notifyWin = notifyWin;
    }

    /// <summary>进入胜利状态时停止常规行为并播放终点表现。</summary>
    public override void OnEnter()
    {
        health.isVpetDead = true;
        coroutineRunner?.StopAllCoroutines();
        stopFalling?.Invoke();
        rigMotion?.SetLyingCollider(false);
        if (animator != null)
            animator.SetBool("isClimbing", false);

        AudioManager.Instance.PlaySound("win");
        AudioManager.Instance.PlaySound3D("setRespawnPoint", ownerTransform.position);
        ObjectPoolManager.GetOrCreate().Spawn(winParticle, ownerTransform.position, Quaternion.identity);
        animator?.SetTrigger("Win");
        notifyWin?.Invoke();
    }

    /// <summary>胜利状态不重复执行普通帧行为。</summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    public override void OnUpdate(float deltaTime)
    {
    }

    /// <summary>胜利状态不执行物理帧行为。</summary>
    public override void OnFixedUpdate()
    {
    }

    #endregion
}

using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 桌宠进食状态
/// - 管理进食表现、食物效果分派、增益协程和随机瞬移。
/// </summary>
public sealed class VpetState_Eat : VpetStateBase
{
    /// <summary>负责运行进食和增益协程的 Unity 宿主。</summary>
    private readonly MonoBehaviour coroutineRunner;

    /// <summary>负责播放进食表现的主体动画器。</summary>
    private readonly Animator animator;

    /// <summary>负责播放手部进食表现的动画器。</summary>
    private readonly Animator handAnimator;

    /// <summary>负责播放被食用物品表现的动画器。</summary>
    private readonly Animator eatenItemAnimator;

    /// <summary>用于显示当前食物图标的渲染器。</summary>
    private readonly SpriteRenderer eatenItemSprite;

    /// <summary>负责生命恢复与受伤处理的生命系统。</summary>
    private readonly VpetHealthSystem health;

    /// <summary>负责一拳效果的攻击对象。</summary>
    private readonly VpetAttack attack;

    /// <summary>负责限时增益倍率的效果对象。</summary>
    private readonly VpetEffect effect;

    /// <summary>负责瞬移落点查询的环境探测器。</summary>
    private readonly VpetEnvironmentSensor environmentSensor;

    /// <summary>负责执行瞬移位置修改的刚体运动对象。</summary>
    private readonly VpetRigMotion rigMotion;

    /// <summary>桌宠世界位置，用于生成特效和计算瞬移候选点。</summary>
    private readonly Transform ownerTransform;

    /// <summary>进食后的状态切换请求。</summary>
    private readonly Action<VpetState> requestState;

    /// <summary>停止当前飘飞流程的回调。</summary>
    private readonly Action stopFallingIfNeeded;

    /// <summary>启动睡眠流程的回调。</summary>
    private readonly Action beginSleep;

    /// <summary>读取当前进食权限的回调。</summary>
    private readonly Func<bool> canEat;

    /// <summary>修改当前进食权限的回调。</summary>
    private readonly Action<bool> setAllowEat;

    /// <summary>提示文本预制体。</summary>
    private readonly GameObject textPrefab;

    /// <summary>提示文本的父级画布。</summary>
    private readonly GameObject figureCanvas;

    /// <summary>速度增益粒子预制体。</summary>
    private readonly GameObject speedUpParticle;

    /// <summary>攻击增益粒子预制体。</summary>
    private readonly GameObject attackUpParticle;

    /// <summary>一拳状态显示对象。</summary>
    private readonly GameObject onePunchState;

    /// <summary>一拳粒子预制体。</summary>
    private readonly GameObject onePunchEffect;

    /// <summary>瞬间爆炸食物使用的炸弹预制体。</summary>
    private readonly GameObject bombPrefab;

    /// <summary>速度增益协程引用，用于刷新同类效果。</summary>
    private Coroutine speedUpBuffCoroutine;

    /// <summary>攻击增益协程引用，用于刷新同类效果。</summary>
    private Coroutine attackUpBuffCoroutine;

    /// <summary>随机瞬移最大范围。</summary>
    private const float TeleportRange = 12f;

    /// <summary>随机瞬移最大查找次数。</summary>
    private const int MaxTeleportSearchAttempts = 10;

    #region 初始化与状态行为

    /// <summary>创建不绑定外部依赖的进食状态占位处理器。</summary>
    public VpetState_Eat() : base(VpetState.Eat)
    {
    }

    /// <summary>
    /// 创建进食状态处理器并注入食物效果所需依赖。
    /// </summary>
    /// <param name="coroutineRunner">用于运行进食和增益协程的 Unity 宿主。</param>
    /// <param name="animator">桌宠主体动画器。</param>
    /// <param name="handAnimator">桌宠手部动画器。</param>
    /// <param name="eatenItemAnimator">被食用物品动画器。</param>
    /// <param name="eatenItemSprite">被食用物品图标渲染器。</param>
    /// <param name="health">桌宠生命系统。</param>
    /// <param name="attack">桌宠攻击对象。</param>
    /// <param name="effect">桌宠效果对象。</param>
    /// <param name="environmentSensor">瞬移落点探测器。</param>
    /// <param name="rigMotion">瞬移位置操作对象。</param>
    /// <param name="ownerTransform">桌宠主体的世界变换。</param>
    /// <param name="requestState">请求状态机切换状态的回调。</param>
    /// <param name="stopFallingIfNeeded">停止飘飞流程的回调。</param>
    /// <param name="beginSleep">启动睡眠流程的回调。</param>
    /// <param name="canEat">读取进食权限的回调。</param>
    /// <param name="setAllowEat">修改进食权限的回调。</param>
    /// <param name="textPrefab">提示文本预制体。</param>
    /// <param name="figureCanvas">提示文本父级画布。</param>
    /// <param name="speedUpParticle">速度增益粒子预制体。</param>
    /// <param name="attackUpParticle">攻击增益粒子预制体。</param>
    /// <param name="onePunchState">一拳状态显示对象。</param>
    /// <param name="onePunchEffect">一拳粒子预制体。</param>
    /// <param name="bombPrefab">瞬间爆炸使用的炸弹预制体。</param>
    public VpetState_Eat(
        MonoBehaviour coroutineRunner,
        Animator animator,
        Animator handAnimator,
        Animator eatenItemAnimator,
        SpriteRenderer eatenItemSprite,
        VpetHealthSystem health,
        VpetAttack attack,
        VpetEffect effect,
        VpetEnvironmentSensor environmentSensor,
        VpetRigMotion rigMotion,
        Transform ownerTransform,
        Action<VpetState> requestState,
        Action stopFallingIfNeeded,
        Action beginSleep,
        Func<bool> canEat,
        Action<bool> setAllowEat,
        GameObject textPrefab,
        GameObject figureCanvas,
        GameObject speedUpParticle,
        GameObject attackUpParticle,
        GameObject onePunchState,
        GameObject onePunchEffect,
        GameObject bombPrefab) : base(VpetState.Eat)
    {
        this.coroutineRunner = coroutineRunner;
        this.animator = animator;
        this.handAnimator = handAnimator;
        this.eatenItemAnimator = eatenItemAnimator;
        this.eatenItemSprite = eatenItemSprite;
        this.health = health;
        this.attack = attack;
        this.effect = effect;
        this.environmentSensor = environmentSensor;
        this.rigMotion = rigMotion;
        this.ownerTransform = ownerTransform;
        this.requestState = requestState;
        this.stopFallingIfNeeded = stopFallingIfNeeded;
        this.beginSleep = beginSleep;
        this.canEat = canEat;
        this.setAllowEat = setAllowEat;
        this.textPrefab = textPrefab;
        this.figureCanvas = figureCanvas;
        this.speedUpParticle = speedUpParticle;
        this.attackUpParticle = attackUpParticle;
        this.onePunchState = onePunchState;
        this.onePunchEffect = onePunchEffect;
        this.bombPrefab = bombPrefab;
    }

    /// <summary>
    /// 在桌宠存活且允许进食时播放进食表现，并启动食物效果协程。
    /// </summary>
    /// <param name="item">待食用的物品数据；为空时不启动流程。</param>
    public void TryEat(ItemData item)
    {
        if (health == null || health.isVpetDead || item == null || !canEat())
            return;

        stopFallingIfNeeded();
        setAllowEat(false);
        eatenItemSprite.sprite = item.icon;
        requestState(VpetState.Eat);
        animator.SetTrigger("Eat");
        handAnimator.SetTrigger("Eat");
        eatenItemAnimator.SetTrigger("Eat");
        AudioManager.Instance.PlaySound("pickItem");
        coroutineRunner.StartCoroutine(StartFoodJudge(item));
    }

    /// <summary>进食状态不额外执行普通帧行为，具体流程由食物协程驱动。</summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    public override void OnUpdate(float deltaTime)
    {
    }

    /// <summary>进食状态不额外执行物理帧行为。</summary>
    public override void OnFixedUpdate()
    {
    }

    #endregion

    #region 食物效果与辅助流程

    /// <summary>
    /// 等待进食动画阶段结束，再按物品编号分派恢复、睡眠、增益或随机事件。
    /// </summary>
    /// <param name="item">由进食入口传入的非空物品数据。</param>
    /// <returns>包含进食等待阶段的协程迭代器。</returns>
    private IEnumerator StartFoodJudge(ItemData item)
    {
        yield return new WaitForSeconds(0.8f);
        AudioManager.Instance.PlaySound("eating");
        yield return new WaitForSeconds(1.6f);

        switch (item.itemID)
        {
            case 0:
                health.VpetRecover(10f);
                requestState(VpetState.Walking);
                setAllowEat(true);
                break;
            case 1:
                beginSleep();
                break;
            case 2:
                attack.GrantOnePunch();
                onePunchState.SetActive(true);
                UnityEngine.Object.Instantiate(onePunchEffect, ownerTransform.position, Quaternion.identity);
                AudioManager.Instance.PlaySound("OnePunchState");
                requestState(VpetState.Walking);
                setAllowEat(true);
                break;
            case 3:
                ResolveEarthEvent();
                setAllowEat(true);
                break;
            case 4:
                StartAttackBuff();
                requestState(VpetState.Walking);
                setAllowEat(true);
                break;
            case 5:
                StartSpeedBuff();
                requestState(VpetState.Walking);
                setAllowEat(true);
                break;
            default:
                Debug.Log("未知食物类型");
                break;
        }
    }

    /// <summary>分派地球食物产生的随机事件。</summary>
    private void ResolveEarthEvent()
    {
        int eventIndex = RandomSelector.Instance.EventRandomSelector(1);
        switch (eventIndex)
        {
            case 1:
                health.VpetRecover(20f);
                requestState(VpetState.Walking);
                break;
            case 2:
                health.VpetGethurt(999f, Vector2.up * 100f);
                requestState(VpetState.Die);
                break;
            case 3:
                StartSpeedBuff();
                requestState(VpetState.Walking);
                break;
            case 4:
                StartAttackBuff();
                requestState(VpetState.Walking);
                break;
            case 5:
                health.VpetGethurt(10f, Vector2.up * 100f);
                requestState(VpetState.Walking);
                break;
            case 6:
                Teleport();
                requestState(VpetState.Walking);
                break;
            case 7:
                GameObject bomb = UnityEngine.Object.Instantiate(bombPrefab, ownerTransform.position, Quaternion.identity);
                bomb.GetComponent<Item_Block_bomb>().isInstanctlyExplode = true;
                requestState(VpetState.Walking);
                break;
        }
    }

    /// <summary>刷新并启动速度增益协程。</summary>
    private void StartSpeedBuff()
    {
        if (speedUpBuffCoroutine != null)
            coroutineRunner.StopCoroutine(speedUpBuffCoroutine);
        speedUpBuffCoroutine = coroutineRunner.StartCoroutine(EatSpeedUp());
    }

    /// <summary>刷新并启动攻击增益协程。</summary>
    private void StartAttackBuff()
    {
        if (attackUpBuffCoroutine != null)
            coroutineRunner.StopCoroutine(attackUpBuffCoroutine);
        attackUpBuffCoroutine = coroutineRunner.StartCoroutine(EatAttackUp());
    }

    /// <summary>
    /// 启用行走与飘飞水平施力增益，生成提示和粒子，并在持续时间结束后还原倍率。
    /// </summary>
    /// <returns>控制加速增益持续时间的协程迭代器。</returns>
    private IEnumerator EatSpeedUp()
    {
        ShowText("速度提升↑↑");
        AudioManager.Instance.PlaySound("getBuff");
        effect.ActivateSpeedBuff();
        GameObject particle = UnityEngine.Object.Instantiate(speedUpParticle, ownerTransform.position, Quaternion.identity, ownerTransform);
        UnityEngine.Object.Destroy(particle, VpetEffect.SpeedBuffDuration);
        yield return new WaitForSeconds(VpetEffect.SpeedBuffDuration);
        effect.ClearSpeedBuff();
        speedUpBuffCoroutine = null;
    }

    /// <summary>
    /// 临时提高普通攻击伤害、缩短攻击间隔并禁止受击击退，结束后恢复默认修正。
    /// </summary>
    /// <returns>控制攻击增益持续时间的协程迭代器。</returns>
    private IEnumerator EatAttackUp()
    {
        AudioManager.Instance.PlaySound("getBuff");
        ShowText("攻击提升↑↑");
        health.SetKnockBack(false);
        GameObject particle = UnityEngine.Object.Instantiate(attackUpParticle, ownerTransform.position, Quaternion.identity, ownerTransform);
        UnityEngine.Object.Destroy(particle, VpetEffect.SpeedBuffDuration);
        effect.ActivateAttackBuff(attack);
        yield return new WaitForSeconds(VpetEffect.AttackBuffDuration);
        effect.ClearAttackBuff(attack);
        health.SetKnockBack(true);
        attackUpBuffCoroutine = null;
    }

    /// <summary>
    /// 在限定范围和尝试次数内寻找可用瞬移位置，并播放音效。
    /// </summary>
    private void Teleport()
    {
        Vector2 targetPosition = Vector2.zero;
        bool foundValidPosition = false;
        for (int i = 0; i < MaxTeleportSearchAttempts; i++)
        {
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * TeleportRange;
            targetPosition = (Vector2)ownerTransform.position + randomDirection;
            if (environmentSensor.CanTeleportTo(targetPosition))
            {
                foundValidPosition = true;
                break;
            }
        }

        if (foundValidPosition)
            rigMotion.TeleportTo(targetPosition);
        else
            rigMotion.TeleportTo(ownerTransform.position + Vector3.up * 0.5f);
        AudioManager.Instance.PlaySound("teleport");
    }

    /// <summary>在提示画布下创建浅黄色提示文本。</summary>
    /// <param name="text">要显示的提示文本。</param>
    private void ShowText(string text)
    {
        Transform parent = figureCanvas.transform;
        GameObject figureText = UnityEngine.Object.Instantiate(textPrefab, ownerTransform.position + Vector3.up, Quaternion.identity, parent);
        TextMeshProUGUI tmp = figureText.GetComponent<TextMeshProUGUI>();
        tmp.SetText(text);
        tmp.color = new Color(1f, 1f, 0.4f, 1f);
    }

    #endregion
}

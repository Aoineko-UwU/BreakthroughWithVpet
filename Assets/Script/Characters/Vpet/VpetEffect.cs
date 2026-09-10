/// <summary>
/// 桌宠效果类
/// - 保存限时加速和攻击增益的配置与运行数值，不创建粒子、播放音效或管理协程。
/// </summary>
public sealed class VpetEffect
{
    #region 加速增益

    /// <summary>加速增益施加给行走与飘飞驱动力的倍率。</summary>
    private const float SpeedBuffMultiplier = 1.7f;

    /// <summary>加速增益的持续时间，单位为秒。</summary>
    public const float SpeedBuffDuration = 12f;

    /// <summary>当前应用于行走和飘飞驱动力的倍率。</summary>
    public float SpeedForceMultiplier { get; private set; } = 1f;

    /// <summary>
    /// 启用加速增益；重复启用只重设倍率，持续时间由入口协程负责重启。
    /// </summary>
    public void ActivateSpeedBuff()
    {
        SpeedForceMultiplier = SpeedBuffMultiplier;
    }

    /// <summary>
    /// 停用加速增益并恢复默认驱动力倍率。
    /// </summary>
    public void ClearSpeedBuff()
    {
        SpeedForceMultiplier = 1f;
    }

    #endregion

    #region 攻击增益

    /// <summary>攻击增益施加给普通攻击伤害的倍率。</summary>
    private const float AttackDamageMultiplier = 2f;

    /// <summary>攻击增益施加给普通攻击冷却的倍率。</summary>
    private const float AttackCooldownMultiplier = 0.5f;

    /// <summary>攻击增益的持续时间，单位为秒。</summary>
    public const float AttackBuffDuration = 12f;

    /// <summary>
    /// 将攻击增益倍率交给攻击协作对象。
    /// </summary>
    /// <param name="attack">需要接收临时伤害与冷却修正的攻击对象。</param>
    public void ActivateAttackBuff(VpetAttack attack)
    {
        attack.SetTemporaryModifiers(AttackDamageMultiplier, AttackCooldownMultiplier);
    }

    /// <summary>
    /// 清除攻击增益倍率，不影响已经开始计算的攻击冷却。
    /// </summary>
    /// <param name="attack">需要恢复默认修正的攻击对象。</param>
    public void ClearAttackBuff(VpetAttack attack)
    {
        attack.ClearTemporaryModifiers();
    }

    #endregion
}

using UnityEngine;

/// <summary>
/// 伤害来源类型
/// - 标识伤害来自哪类玩法行为，便于后续扩展抗性与受击表现。
/// </summary>
public enum DamageSourceType
{
    Unknown,
    VpetAttack,
    VpetDance,
    Enemy,
    Spike,
    StoneCone,
    Bomb,
    Pearl,
    FoodEvent
}

/// <summary>
/// 伤害信息结构体
/// - 携带伤害数值、来源、命中位置及精确的击退施力方式。
/// </summary>
public readonly struct DamageInfo
{
    /// <summary>本次伤害数值。</summary>
    public float Amount { get; }

    /// <summary>伤害来源类型。</summary>
    public DamageSourceType SourceType { get; }

    /// <summary>命中目标的世界坐标。</summary>
    public Vector2 HitPoint { get; }

    /// <summary>击退方向；无击退时为零向量。</summary>
    public Vector2 Direction => Knockback.normalized;

    /// <summary>施加到刚体上的原始力向量。</summary>
    public Vector2 Knockback { get; }

    /// <summary>施力模式，保留不同受伤对象原有的力学语义。</summary>
    public ForceMode2D ForceMode { get; }

    /// <summary>施力前是否清空刚体速度。</summary>
    public bool ResetVelocity { get; }

    /// <summary>
    /// 创建一次伤害请求；击退系数及无敌判定仍由受击对象自身负责。
    /// </summary>
    /// <param name="amount">伤害数值。</param>
    /// <param name="sourceType">伤害来源类型。</param>
    /// <param name="hitPoint">命中目标的世界坐标。</param>
    /// <param name="knockback">原始施力向量。</param>
    /// <param name="forceMode">刚体施力模式。</param>
    /// <param name="resetVelocity">施力前是否清空速度。</param>
    public DamageInfo(float amount, DamageSourceType sourceType, Vector2 hitPoint,
        Vector2 knockback, ForceMode2D forceMode = ForceMode2D.Force, bool resetVelocity = false)
    {
        Amount = amount;
        SourceType = sourceType;
        HitPoint = hitPoint;
        Knockback = knockback;
        ForceMode = forceMode;
        ResetVelocity = resetVelocity;
    }

    /// <summary>
    /// 按来源与目标的世界坐标生成远离来源的冲量，沿用敌人原有的三维方向归一化算法。
    /// </summary>
    /// <param name="amount">伤害数值。</param>
    /// <param name="sourceType">伤害来源类型。</param>
    /// <param name="sourcePosition">伤害来源的世界坐标。</param>
    /// <param name="targetPosition">受击目标的世界坐标。</param>
    /// <param name="force">击退冲量大小。</param>
    /// <returns>携带敌人原有击退语义的伤害信息。</returns>
    public static DamageInfo FromSource(float amount, DamageSourceType sourceType,
        Vector3 sourcePosition, Vector3 targetPosition, float force)
    {
        Vector2 knockback = (targetPosition - sourcePosition).normalized * force;
        return new DamageInfo(amount, sourceType, targetPosition, knockback, ForceMode2D.Impulse);
    }
}

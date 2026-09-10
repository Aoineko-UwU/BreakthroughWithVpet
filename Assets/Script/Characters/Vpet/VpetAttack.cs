using UnityEngine;

/// <summary>
/// 桌宠攻击类
/// - 管理普通攻击、跳舞范围攻击、一拳状态、伤害修正和攻击冷却，不负责动画、音效或状态切换。
/// </summary>
public sealed class VpetAttack
{
    #region 基础属性与运行状态

    /// <summary>普通攻击的基础伤害，由难度初始化。</summary>
    private float baseDamage = 3f;

    /// <summary>普通攻击的基础冷却，单位为秒，由难度初始化。</summary>
    private float baseCooldown = 1.5f;

    /// <summary>普通攻击距离下次可发起的剩余时间。</summary>
    private float normalAttackTimer;

    /// <summary>攻击增益对普通攻击伤害的修正倍率。</summary>
    private float damageMultiplier = 1f;

    /// <summary>攻击增益对普通攻击冷却的修正倍率。</summary>
    private float cooldownMultiplier = 1f;

    /// <summary>下一次命中敌人的普通攻击是否使用一拳伤害。</summary>
    public bool HasOnePunch { get; private set; }

    /// <summary>跳舞范围攻击的冷却，单位为秒。</summary>
    private float danceAttackTimer;

    /// <summary>跳舞范围攻击成功命中后的冷却，单位为秒。</summary>
    private const float DanceAttackCooldown = 0.5f;

    /// <summary>跳舞范围攻击对每个命中敌人的伤害。</summary>
    private const float DanceDamage = 3f;

    /// <summary>跳舞范围攻击的重叠查询半径。</summary>
    private const float DanceRadius = 2f;

    /// <summary>跳舞范围攻击传给敌人的击退力大小。</summary>
    private const float DanceKnockbackForce = 3f;

    #endregion

    #region 配置与计时

    /// <summary>
    /// 覆盖当前难度对应的普通攻击基础属性；不会重置已在运行的冷却。
    /// </summary>
    /// <param name="damage">普通攻击基础伤害。</param>
    /// <param name="cooldown">普通攻击基础冷却，单位为秒。</param>
    public void ConfigureBaseValues(float damage, float cooldown)
    {
        baseDamage = damage;
        baseCooldown = cooldown;
    }

    /// <summary>
    /// 逐帧递减普通攻击与跳舞攻击的冷却；保持原有计时器允许降至负数的语义。
    /// </summary>
    /// <param name="deltaTime">本帧间隔时间。</param>
    public void Tick(float deltaTime)
    {
        normalAttackTimer -= deltaTime;
        danceAttackTimer -= deltaTime;
    }

    /// <summary>
    /// 设置或清除普通攻击的临时伤害和冷却修正。
    /// </summary>
    /// <param name="newDamageMultiplier">应用于基础伤害的倍率。</param>
    /// <param name="newCooldownMultiplier">应用于基础冷却的倍率。</param>
    public void SetTemporaryModifiers(float newDamageMultiplier, float newCooldownMultiplier)
    {
        damageMultiplier = newDamageMultiplier;
        cooldownMultiplier = newCooldownMultiplier;
    }

    /// <summary>
    /// 恢复普通攻击的默认伤害和冷却修正倍率。
    /// </summary>
    public void ClearTemporaryModifiers()
    {
        damageMultiplier = 1f;
        cooldownMultiplier = 1f;
    }

    /// <summary>
    /// 赋予下一次实际命中敌人的普通攻击一拳状态。
    /// </summary>
    public void GrantOnePunch()
    {
        HasOnePunch = true;
    }

    #endregion

    #region 普通攻击

    /// <summary>
    /// 判断普通攻击是否不在冷却中。
    /// </summary>
    /// <returns>冷却剩余时间不大于零时为 true。</returns>
    public bool CanStartNormalAttack()
    {
        return normalAttackTimer <= 0f;
    }

    /// <summary>
    /// 重置普通攻击冷却；调用方应保持原有时机，在目标组件为空时也照常调用。
    /// </summary>
    public void StartNormalAttackCooldown()
    {
        normalAttackTimer = baseCooldown * cooldownMultiplier;
    }

    /// <summary>
    /// 对指定敌人应用普通攻击伤害，并在一拳状态存在时消耗该状态。
    /// </summary>
    /// <param name="enemy">已经确认存在的敌人生命系统。</param>
    /// <param name="origin">桌宠当前世界坐标，用于敌人计算击退方向。</param>
    /// <returns>本次是否消耗了一拳状态，供入口决定对应表现。</returns>
    public bool ApplyNormalAttack(EnemyHealthSystem enemy, Vector3 origin)
    {
        bool consumedOnePunch = HasOnePunch;
        if (consumedOnePunch)
        {
            HasOnePunch = false;
            enemy.GetHurt(baseDamage * 999f * damageMultiplier, origin, 25f);
        }
        else
        {
            enemy.GetHurt(baseDamage * damageMultiplier, origin, 5f);
        }

        return consumedOnePunch;
    }

    #endregion

    #region 跳舞范围攻击

    /// <summary>
    /// 在跳舞攻击冷却结束时查询范围内敌人并施加伤害；仅在至少命中一名敌人后刷新冷却。
    /// </summary>
    /// <param name="origin">范围查询中心及敌人击退来源。</param>
    /// <param name="enemyLayer">参与范围查询的敌人层掩码。</param>
    /// <returns>本次是否至少对一名敌人发出伤害请求。</returns>
    public bool TryPerformDanceAttack(Vector3 origin, LayerMask enemyLayer)
    {
        if (danceAttackTimer > 0f)
            return false;

        bool didHit = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, DanceRadius, enemyLayer);
        foreach (Collider2D collider in colliders)
        {
            if (collider != null)
            {
                // 保持旧实现：敌人层上应挂有 EnemyHealthSystem，由场景配置保证。
                collider.gameObject.GetComponent<EnemyHealthSystem>().GetHurt(DanceDamage, origin, DanceKnockbackForce);
                didHit = true;
            }
        }

        if (didHit)
            danceAttackTimer = DanceAttackCooldown;

        return didHit;
    }

    #endregion
}

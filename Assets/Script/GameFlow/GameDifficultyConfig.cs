using UnityEngine;

/// <summary>
/// 游戏难度配置类
/// - 集中保存单个难度下的战斗、敌人、道具和环境数值。
/// </summary>
[CreateAssetMenu(fileName = "NewGameDifficultyConfig", menuName = "Game/Difficulty Config")]
public sealed class GameDifficultyConfig : ScriptableObject
{
    #region 桌宠数值

    [Tooltip("桌宠最大生命值。")]
    public float vpetMaxHealth;

    [Tooltip("桌宠受击后的无敌时间，单位为秒。")]
    public float vpetInvincibleTime;

    [Tooltip("桌宠生命恢复倍率。")]
    public float vpetRecoverMultiplier;

    [Tooltip("桌宠普通攻击基础伤害。")]
    public float vpetAttackDamage;

    [Tooltip("桌宠普通攻击基础间隔，单位为秒。")]
    public float vpetAttackCooldown;

    [Tooltip("桌宠接触尖刺时受到的伤害。")]
    public float vpetSpikeDamage;

    #endregion

    #region 敌人与生成数值

    [Tooltip("敌人基础生命值的倍率。")]
    public float enemyHealthMultiplier;

    [Tooltip("青蛙接触攻击伤害。")]
    public float frogAttackDamage;

    [Tooltip("蝙蝠接触攻击伤害。")]
    public float batAttackDamage;

    [Tooltip("蝙蝠移动速度。")]
    public float batMoveSpeed;

    [Tooltip("熊接触攻击伤害。")]
    public float bearAttackDamage;

    [Tooltip("熊移动速度。")]
    public float bearMoveSpeed;

    [Tooltip("熊随机停顿时长的修正值，单位为秒。")]
    public float bearStopTimeFix;

    [Tooltip("敌人重生等待时间的修正值，单位为秒。")]
    public float spawnTimeFix;

    #endregion

    #region 道具与环境数值

    [Tooltip("物品栏自动补充物品的间隔，单位为秒。")]
    public float itemAddCooldown;

    [Tooltip("石锥命中桌宠时的伤害。")]
    public float stoneDamageToVpet;

    [Tooltip("石锥命中方块时的伤害。")]
    public float stoneDamageToBlock;

    #endregion
}

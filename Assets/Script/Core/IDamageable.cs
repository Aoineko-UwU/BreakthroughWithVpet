/// <summary>
/// 可受伤对象接口
/// - 为桌宠、敌人及可破坏物提供统一的伤害入口。
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 接收伤害请求；是否无敌、如何扣血及播放反馈由实现类决定。
    /// </summary>
    /// <param name="damage">本次伤害及击退信息。</param>
    void TakeDamage(DamageInfo damage);
}

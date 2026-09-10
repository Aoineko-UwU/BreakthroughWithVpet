using UnityEngine;

/// <summary>
/// 桌宠环境探测类
/// - 查询地面接触、向下射线与瞬移重叠信息，不控制运动或切换行为状态。
/// </summary>
public sealed class VpetEnvironmentSensor
{
    #region 查询依赖与探测结果

    /// <summary>用于读取接触点的桌宠刚体。</summary>
    private readonly Rigidbody2D body;

    /// <summary>用于读取瞬移检测尺寸的桌宠胶囊碰撞体。</summary>
    private readonly CapsuleCollider2D capsuleCollider;

    /// <summary>用于累计行走法线的接触点缓冲区，保持原有十个接触点上限。</summary>
    private readonly ContactPoint2D[] contactPoints = new ContactPoint2D[10];

    /// <summary>用于确认存在任意地面接触的单元素缓冲区。</summary>
    private readonly ContactPoint2D[] allContactPoints = new ContactPoint2D[1];

    /// <summary>仅用于行走法线计算的地面接触过滤器。</summary>
    private ContactFilter2D groundContactFilter;

    /// <summary>用于判断任意地面接触的过滤器。</summary>
    private ContactFilter2D allGroundContactFilter;

    /// <summary>向下探测的射线长度，保持原有世界单位数值。</summary>
    private const float RayLength = 1.6f;

    /// <summary>左右射线相对中心的水平偏移。</summary>
    private const float HalfWidth = 0.38f;

    /// <summary>最近一次向下射线是否命中；不等同于刚体实际接地。</summary>
    public bool IsGrounded { get; private set; }

    /// <summary>最近一次首条命中射线是否命中 Water 标签；不等同于角色浸水。</summary>
    public bool IsInWater { get; private set; }

    #endregion

    #region 初始化

    /// <summary>
    /// 保存查询所需的组件；接触过滤器由入口在 Start 阶段初始化。
    /// </summary>
    /// <param name="body">桌宠自身的非空二维刚体。</param>
    /// <param name="capsuleCollider">桌宠自身的非空胶囊碰撞体。</param>
    public VpetEnvironmentSensor(Rigidbody2D body, CapsuleCollider2D capsuleCollider)
    {
        this.body = body;
        this.capsuleCollider = capsuleCollider;
    }

    /// <summary>
    /// 按原有层掩码初始化接触查询，均排除触发器。
    /// </summary>
    /// <param name="groundLayer">参与行走平均法线计算的层掩码。</param>
    /// <param name="allGroundLayer">用于确认地面接触的完整层掩码。</param>
    public void InitializeContactFilters(LayerMask groundLayer, LayerMask allGroundLayer)
    {
        groundContactFilter = new ContactFilter2D();
        groundContactFilter.SetLayerMask(groundLayer);
        groundContactFilter.useTriggers = false;

        allGroundContactFilter = new ContactFilter2D();
        allGroundContactFilter.SetLayerMask(allGroundLayer);
        allGroundContactFilter.useTriggers = false;
    }

    #endregion

    #region 地面与水面查询

    /// <summary>
    /// 计算过滤后的接触点平均法线，不修改缓存的射线探测状态。
    /// </summary>
    /// <returns>归一化后的平均法线；没有接触点时返回向上方向。</returns>
    public Vector2 GetAverageGroundNormal()
    {
        int count = body.GetContacts(groundContactFilter, contactPoints);
        if (count == 0)
            return Vector2.up;

        Vector2 sum = Vector2.zero;
        for (int i = 0; i < count; i++)
            sum += contactPoints[i].normal;
        return (sum / count).normalized;
    }

    /// <summary>
    /// 查询刚体是否接触完整地面层中的非触发碰撞体。
    /// </summary>
    /// <returns>存在任意匹配接触点时为 true，不区分该接触面是否可站立。</returns>
    public bool HasGroundContact()
    {
        return body.GetContacts(allGroundContactFilter, allContactPoints) > 0;
    }

    /// <summary>
    /// 按中、左、右顺序向下发射射线，首条命中后停止，并更新地面与水面标记。
    /// <para>由 VpetAction 在原有 Update 位置调用，不额外提前到物理帧刷新。</para>
    /// </summary>
    /// <param name="centerOrigin">中心射线起点的世界坐标。</param>
    /// <param name="allGroundLayer">本次射线使用的完整层掩码，保留原字段运行时变化的语义。</param>
    public void RefreshGround(Vector2 centerOrigin, LayerMask allGroundLayer)
    {
        Vector2 leftOrigin = centerOrigin + Vector2.left * HalfWidth;
        Vector2 rightOrigin = centerOrigin + Vector2.right * HalfWidth;
        IsGrounded = false;
        IsInWater = false;

        // 保留原有探测顺序、首命中规则和全局触发器查询设置。
        foreach (Vector2 origin in new[] { centerOrigin, leftOrigin, rightOrigin })
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, RayLength, allGroundLayer);
            if (hit.collider != null)
            {
                IsGrounded = true;
                if (hit.collider.CompareTag("Water"))
                    IsInWater = true;
                break;
            }
        }
    }

    #endregion

    #region 瞬移落点查询

    /// <summary>
    /// 使用当前胶囊尺寸的五分之一检查瞬移候选位置。
    /// <para>仅保留原有单次重叠查询及 Ignore 例外，不提供完整落点安全保证。</para>
    /// </summary>
    /// <param name="targetPosition">候选位置的世界坐标。</param>
    /// <returns>未查到碰撞体或返回的碰撞体带有 Ignore 标签时为 true。</returns>
    public bool CanTeleportTo(Vector2 targetPosition)
    {
        Collider2D hit = Physics2D.OverlapCapsule(targetPosition, capsuleCollider.size * 0.2f, capsuleCollider.direction, 0f);
        if (hit != null && !hit.CompareTag("Ignore"))
            return false;
        return true;
    }

    #endregion
}

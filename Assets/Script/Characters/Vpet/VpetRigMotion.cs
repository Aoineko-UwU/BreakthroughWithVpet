using UnityEngine;

/// <summary>
/// 桌宠刚体运动类
/// - 执行行走、攀爬、飘飞、位置与碰撞体操作，不判断行为状态或播放表现。
/// </summary>
public sealed class VpetRigMotion
{
    #region 组件依赖与运动参数

    /// <summary>接收驱动力和速度操作的桌宠刚体。</summary>
    private readonly Rigidbody2D body;

    /// <summary>飘飞期间提供持续升力的组件。</summary>
    private readonly ConstantForce2D floatingForce;

    /// <summary>随躺卧状态切换形状的胶囊碰撞体。</summary>
    private readonly CapsuleCollider2D capsuleCollider;

    /// <summary>沿接触面切线的目标行走速度。</summary>
    private const float WalkSpeed = 5.5f;

    /// <summary>向上的目标攀爬速度。</summary>
    private const float ClimbSpeed = 7f;

    /// <summary>攀爬速度差转换为驱动力的倍率。</summary>
    private const float ClimbForceMultiplier = 2f;

    /// <summary>飘飞期间持续向上的力。</summary>
    private const float FloatingVerticalForce = 7.4f;

    /// <summary>飘飞期间的目标水平速度。</summary>
    private const float FloatingHorizontalSpeed = 4f;

    /// <summary>飘飞水平速度差的施力倍率。</summary>
    private const float FloatingForceMultiplier = 2f;

    /// <summary>当前水平速度向左时的额外修正倍率。</summary>
    private const float NegativeVelocityMultiplier = 0.08f;

    #endregion

    #region 初始化

    /// <summary>
    /// 保存桌宠已有的物理组件，不添加组件或自行订阅 Unity 生命周期。
    /// </summary>
    /// <param name="body">桌宠自身的非空二维刚体。</param>
    /// <param name="floatingForce">桌宠自身的非空持续力组件。</param>
    /// <param name="capsuleCollider">桌宠自身的非空胶囊碰撞体。</param>
    public VpetRigMotion(Rigidbody2D body, ConstantForce2D floatingForce, CapsuleCollider2D capsuleCollider)
    {
        this.body = body;
        this.floatingForce = floatingForce;
        this.capsuleCollider = capsuleCollider;
    }

    #endregion

    #region 行走与攀爬驱动

    /// <summary>
    /// 沿地面切线施加行走力，并保留现有重力补偿及环境修正算法。
    /// <para>本方法不消除旧算法的坡面卡住问题，也不改变重力缩放的处理方式。</para>
    /// </summary>
    /// <param name="groundNormal">环境探测器提供的平均接触法线。</param>
    /// <param name="isInWater">最近一次向下射线是否首先命中水面。</param>
    /// <param name="hasGroundContact">当前是否存在完整地面层的刚体接触点。</param>
    /// <param name="isGrounded">最近一次向下射线是否命中地面。</param>
    /// <param name="speedMultiplier">增益对行走驱动力的倍率，不修改目标速度。</param>
    public void Walk(Vector2 groundNormal, bool isInWater, bool hasGroundContact, bool isGrounded, float speedMultiplier)
    {
        Vector2 tangent = new Vector2(groundNormal.y, -groundNormal.x).normalized;
        if (Mathf.Abs(groundNormal.x) > 0.9f && Mathf.Abs(groundNormal.y) < 0.1f)
            tangent = Vector2.right;

        Vector2 gravityForce = body.mass * Physics2D.gravity;
        float gravityAlongTangent = Vector2.Dot(gravityForce, tangent);
        Vector2 compensationForce = -gravityAlongTangent * tangent;

        // 维持按法线竖直分量选取补偿比例的旧规则；它不能区分上坡和下坡。
        if (groundNormal.y > 0)
            body.AddForce(compensationForce * 0.6f, ForceMode2D.Force);
        else if (groundNormal.y < 0)
            body.AddForce(compensationForce, ForceMode2D.Force);

        float forceMultiplier = isInWater ? 0.8f : hasGroundContact ? 2f : isGrounded ? 0.6f : 0f;
        Vector2 desiredVelocity = tangent * WalkSpeed;
        Vector2 force = (desiredVelocity - body.velocity) * forceMultiplier * speedMultiplier;
        body.AddForce(force, ForceMode2D.Force);
    }

    /// <summary>
    /// 清零刚体速度，供首次进入攀爬阶段时使用。
    /// </summary>
    public void ResetVelocity()
    {
        body.velocity = Vector2.zero;
    }

    /// <summary>
    /// 根据向上目标速度与当前速度之差施加攀爬力。
    /// </summary>
    public void Climb()
    {
        Vector2 desiredVelocity = Vector2.up * ClimbSpeed;
        Vector2 climbForce = (desiredVelocity - body.velocity) * ClimbForceMultiplier;
        body.AddForce(climbForce, ForceMode2D.Force);
    }

    /// <summary>
    /// 离开梯子时施加原有的向上推力；保持 Force 模式而非改成瞬时冲量。
    /// </summary>
    public void PushOffLadder()
    {
        body.AddForce(Vector2.up * 200f, ForceMode2D.Force);
    }

    #endregion

    #region 飘飞驱动

    /// <summary>
    /// 开始施加飘飞阶段的持续升力。
    /// </summary>
    public void StartFloating()
    {
        floatingForce.force = Vector2.up * FloatingVerticalForce;
    }

    /// <summary>
    /// 停止持续升力，不清零已有速度。
    /// </summary>
    public void StopFloating()
    {
        floatingForce.force = Vector2.zero;
    }

    /// <summary>
    /// 在飘飞阶段修正水平速度；已有向左速度时减弱向右的驱动力。
    /// </summary>
    /// <param name="speedMultiplier">增益对水平驱动力的倍率。</param>
    public void DriveFloatingHorizontal(float speedMultiplier)
    {
        float horizontalVelocity = body.velocity.x;
        float speedDifference = FloatingHorizontalSpeed - horizontalVelocity;
        float multiplier = horizontalVelocity < 0
            ? FloatingForceMultiplier * NegativeVelocityMultiplier
            : FloatingForceMultiplier;
        float forceX = speedDifference * multiplier * speedMultiplier;
        body.AddForce(Vector2.right * forceX, ForceMode2D.Force);
    }

    #endregion

    #region 位置与碰撞体

    /// <summary>
    /// 直接设置桌宠 Transform 的世界位置，保留原有瞬移方式且不重置速度。
    /// </summary>
    /// <param name="position">目标世界坐标，调用方负责落点检查。</param>
    public void TeleportTo(Vector3 position)
    {
        body.transform.position = position;
    }

    /// <summary>
    /// 将胶囊切换为躺卧或站立形状，不在此处判断游戏状态。
    /// </summary>
    /// <param name="isLyingDown">睡眠或死亡时为 true，其他状态为 false。</param>
    public void SetLyingCollider(bool isLyingDown)
    {
        if (isLyingDown)
        {
            capsuleCollider.offset = new Vector2(0, -2.67f);
            capsuleCollider.size = new Vector2(9f, 3.6f);
            capsuleCollider.direction = CapsuleDirection2D.Horizontal;
        }
        else
        {
            capsuleCollider.offset = Vector2.zero;
            capsuleCollider.size = new Vector2(3.67f, 9.2f);
            capsuleCollider.direction = CapsuleDirection2D.Vertical;
        }
    }

    #endregion
}

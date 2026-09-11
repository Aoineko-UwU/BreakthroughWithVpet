/// <summary>
/// 桌宠待机状态
/// - 作为状态机中的待机状态处理器，当前不主动执行行为。
/// </summary>
public sealed class VpetState_Idle : VpetStateBase
{
    #region 初始化

    /// <summary>创建待机状态处理器。</summary>
    public VpetState_Idle() : base(VpetState.Idle)
    {
    }

    #endregion
}

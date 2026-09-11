/// <summary>
/// 桌宠状态枚举
/// - 定义桌宠行为及结算状态，编号继续兼容现有外部状态入口。
/// </summary>
public enum VpetState
{
    /// <summary>待机状态，外部编号为 0。</summary>
    Idle,

    /// <summary>行走状态，外部编号为 1。</summary>
    Walking,

    /// <summary>飘飞状态，外部编号为 2。</summary>
    Fall,

    /// <summary>攀爬状态，外部编号为 3。</summary>
    Climb,

    /// <summary>进食状态，外部编号为 4。</summary>
    Eat,

    /// <summary>睡眠状态，外部编号为 5。</summary>
    Sleep,

    /// <summary>跳舞状态，外部编号为 6。</summary>
    Dance,

    /// <summary>死亡状态，外部编号为 7。</summary>
    Die,

    /// <summary>胜利状态，外部编号为 8。</summary>
    Win
}

/// <summary>
/// 桌宠状态处理器接口
/// - 定义状态对象的身份以及进入、退出生命周期。
/// </summary>
public interface IVpetStateHandler
{
    /// <summary>当前处理器对应的状态。</summary>
    VpetState State { get; }

    /// <summary>进入该状态时调用。</summary>
    void OnEnter();

    /// <summary>离开该状态时调用。</summary>
    void OnExit();

    /// <summary>
    /// 在普通帧执行当前状态行为。
    /// </summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    void OnUpdate(float deltaTime);

    /// <summary>在物理帧执行当前状态行为。</summary>
    void OnFixedUpdate();
}

/// <summary>
/// 桌宠状态处理器基类
/// - 提供具体状态共享的状态标识和暂不带行为的生命周期默认实现。
/// </summary>
public abstract class VpetStateBase : IVpetStateHandler
{
    /// <summary>当前处理器对应的状态。</summary>
    public VpetState State { get; }

    /// <summary>
    /// 创建指定状态的基础处理器。
    /// </summary>
    /// <param name="state">该处理器负责的状态。</param>
    protected VpetStateBase(VpetState state)
    {
        State = state;
    }

    /// <summary>
    /// 状态进入时的默认钩子，留给后续具体状态实现行为。
    /// </summary>
    public virtual void OnEnter()
    {
    }

    /// <summary>
    /// 状态退出时的默认钩子，留给后续具体状态实现行为。
    /// </summary>
    public virtual void OnExit()
    {
    }

    /// <summary>
    /// 状态普通帧行为的默认钩子，留给具体状态实现非物理逻辑。
    /// </summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    public virtual void OnUpdate(float deltaTime)
    {
    }

    /// <summary>状态物理帧行为的默认钩子，留给具体状态实现刚体相关逻辑。</summary>
    public virtual void OnFixedUpdate()
    {
    }
}

/// <summary>
/// 桌宠待机状态
/// - 作为状态机中的待机状态处理器占位。
/// </summary>
public sealed class VpetState_Idle : VpetStateBase
{
    /// <summary>创建待机状态处理器。</summary>
    public VpetState_Idle() : base(VpetState.Idle) { }
}

/// <summary>
/// 桌宠飘飞状态
/// - 作为状态机中的飘飞状态处理器占位。
/// </summary>
public sealed class VpetState_Fall : VpetStateBase
{
    /// <summary>创建飘飞状态处理器。</summary>
    public VpetState_Fall() : base(VpetState.Fall) { }
}

/// <summary>
/// 桌宠进食状态
/// - 作为状态机中的进食状态处理器占位。
/// </summary>
public sealed class VpetState_Eat : VpetStateBase
{
    /// <summary>创建进食状态处理器。</summary>
    public VpetState_Eat() : base(VpetState.Eat) { }
}

/// <summary>
/// 桌宠睡眠状态
/// - 作为状态机中的睡眠状态处理器占位。
/// </summary>
public sealed class VpetState_Sleep : VpetStateBase
{
    /// <summary>创建睡眠状态处理器。</summary>
    public VpetState_Sleep() : base(VpetState.Sleep) { }
}

/// <summary>
/// 桌宠跳舞状态
/// - 作为状态机中的跳舞状态处理器占位。
/// </summary>
public sealed class VpetState_Dance : VpetStateBase
{
    /// <summary>创建跳舞状态处理器。</summary>
    public VpetState_Dance() : base(VpetState.Dance) { }
}

/// <summary>
/// 桌宠死亡状态
/// - 作为状态机中的死亡状态处理器占位。
/// </summary>
public sealed class VpetState_Die : VpetStateBase
{
    /// <summary>创建死亡状态处理器。</summary>
    public VpetState_Die() : base(VpetState.Die) { }
}

/// <summary>
/// 桌宠胜利状态
/// - 作为状态机中的胜利状态处理器占位。
/// </summary>
public sealed class VpetState_Win : VpetStateBase
{
    /// <summary>创建胜利状态处理器。</summary>
    public VpetState_Win() : base(VpetState.Win) { }
}

/// <summary>
/// 桌宠状态机
/// - 注册全部桌宠状态处理器，维护当前状态并负责进入与退出回调。
/// </summary>
public sealed class VpetStateMachine
{
    #region 状态注册与当前状态

    /// <summary>按状态编号保存的具体处理器。</summary>
    private readonly System.Collections.Generic.Dictionary<VpetState, IVpetStateHandler> handlers = new();

    /// <summary>当前生效的桌宠状态。</summary>
    public VpetState CurrentState { get; private set; }

    #endregion

    #region 初始化与切换

    /// <summary>
    /// 注册全部桌宠状态处理器，并以指定状态作为初始状态；初始化时不触发进入回调。
    /// </summary>
    /// <param name="initialState">状态机的初始状态。</param>
    /// <param name="overrides">用于替换默认处理器的状态实现，可为空。</param>
    public VpetStateMachine(VpetState initialState, params IVpetStateHandler[] overrides)
    {
        Register(new VpetState_Idle());
        Register(new VpetState_Walking());
        Register(new VpetState_Fall());
        Register(new VpetState_Climb());
        Register(new VpetState_Eat());
        Register(new VpetState_Sleep());
        Register(new VpetState_Dance());
        Register(new VpetState_Die());
        Register(new VpetState_Win());

        if (overrides != null)
        {
            foreach (IVpetStateHandler handler in overrides)
                Register(handler);
        }

        CurrentState = initialState;
    }

    /// <summary>
    /// 注册一个状态处理器；同一状态编号后注册的处理器会覆盖先前注册项。
    /// </summary>
    /// <param name="handler">待注册的非空状态处理器。</param>
    private void Register(IVpetStateHandler handler)
    {
        handlers[handler.State] = handler;
    }

    /// <summary>
    /// 切换到指定状态，并按顺序调用旧状态退出和新状态进入回调。
    /// </summary>
    /// <param name="nextState">要切换到的状态。</param>
    /// <returns>状态编号已注册并完成处理时为 true；未知状态时为 false。</returns>
    public bool SetState(VpetState nextState)
    {
        if (!handlers.TryGetValue(nextState, out IVpetStateHandler nextHandler))
            return false;

        if (CurrentState == nextState)
            return true;

        handlers[CurrentState].OnExit();
        CurrentState = nextState;
        nextHandler.OnEnter();
        return true;
    }

    /// <summary>执行当前状态的普通帧行为。</summary>
    /// <param name="deltaTime">当前帧经过的时间，单位为秒。</param>
    public void Update(float deltaTime)
    {
        handlers[CurrentState].OnUpdate(deltaTime);
    }

    /// <summary>执行当前状态的物理帧行为。</summary>
    public void FixedUpdate()
    {
        handlers[CurrentState].OnFixedUpdate();
    }

    #endregion
}

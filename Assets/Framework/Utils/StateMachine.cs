public class StateMachine<T> where T : IState
{
    public T State { get; set; }

    public void SetState(T state)
    {
        State?.Disable();
        state.Enable();
        State = state;
    }
}

public interface IState
{
    void Enable();
    void Disable();
}

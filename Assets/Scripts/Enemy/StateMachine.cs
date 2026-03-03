using System.Linq;

public class StateMachine
{
    private IState[] _states;
    private IState _current;

    public StateMachine(params IState[] states)
    {
        _states = states;
    }

    public void SetState<T>() where T : IState
    {
        _current?.Exit();
        _current = _states.First(s => s is T);
        _current?.Enter();
    }

    public void Tick(float deltaTime)
    {
        _current?.Tick(deltaTime);
    }
}


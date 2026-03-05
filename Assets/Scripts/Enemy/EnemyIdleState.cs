public class EnemyIdleState : IState
{
    private readonly Enemy _enemy;

    public EnemyIdleState(Enemy enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {
        
    }

    public void Exit()
    {
        
    }

    public void Tick(float deltaTime)
    {
        _enemy.AssignTarget();

        if (_enemy.HasAliveTarget())
            _enemy.StateMachine.SetState<EnemyChaseState>();
    }
}


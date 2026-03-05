public class EnemyIdleState : IState
{
    private readonly Enemy _enemy;

    public EnemyIdleState(Enemy enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {
        UnityEngine.Debug.Log($"[EnemyState] Enter Idle (EnemyId: {_enemy.Id})");
    }

    public void Exit()
    {
        UnityEngine.Debug.Log($"[EnemyState] Exit Idle (EnemyId: {_enemy.Id})");
    }

    public void Tick(float deltaTime)
    {
        _enemy.AssignTarget();

        if (_enemy.HasAliveTarget())
            _enemy.StateMachine.SetState<EnemyChaseState>();
    }
}


using UnityEngine;

public class EnemyChaseState : IState
{
    private readonly Enemy _enemy;

    public EnemyChaseState(Enemy enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {
        Debug.Log($"[EnemyState] Enter Chase (EnemyId: {_enemy.Id})");
    }

    public void Exit()
    {
        Debug.Log($"[EnemyState] Exit Chase (EnemyId: {_enemy.Id})");
    }

    public void Tick(float deltaTime)
    {
        if (!_enemy.HasAliveTarget())
        {
            _enemy.AssignTarget();
            if (!_enemy.HasAliveTarget())
            {
                _enemy.StateMachine.SetState<EnemyIdleState>();
                return;
            }
        }

        Vector3 dir =
            _enemy.Target.position - _enemy.View.transform.position;

        float distance = dir.magnitude;
        dir.Normalize();

        Rotate(dir,deltaTime);

        if (distance > _enemy.AttackDistance)
        {
            Move(deltaTime);
        }
        else
        {
            _enemy.StateMachine.SetState<EnemyAttackState>();
        }
    }
    
    private void Rotate(Vector3 dir, float deltaTime)
    {
        Quaternion target = Quaternion.LookRotation(dir);

        _enemy.View.transform.rotation =
            Quaternion.RotateTowards(
                _enemy.View.transform.rotation,
                target,
                _enemy.RotationSpeed * deltaTime);
    }

    private void Move(float deltaTime)
    {
        _enemy.View.transform.position += _enemy.View.transform.forward * _enemy.MoveSpeed * deltaTime;
    }

}


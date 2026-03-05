using UnityEngine;

public class EnemyAttackState : IState
{
    private readonly Enemy _enemy;

    public EnemyAttackState(Enemy enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {
        Debug.Log($"[EnemyState] Enter Attack (EnemyId: {_enemy.Id})");
        _enemy.AttackTimer = 0f;
    }

    public void Exit()
    {
        Debug.Log($"[EnemyState] Exit Attack (EnemyId: {_enemy.Id})");
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

        Vector3 dir = _enemy.Target.position - _enemy.View.transform.position;

        float distance = dir.magnitude;

        if (distance > _enemy.AttackDistance)
        {
            _enemy.StateMachine.SetState<EnemyIdleState>();
            return;
        }

        _enemy.AttackTimer += deltaTime;

        if (_enemy.AttackTimer >= _enemy.AttackCooldown)
        {
            _enemy.AttackTimer = 0f;

            if (_enemy.TargetStats != null)
                _enemy.PlayerDamageService.ApplyDamage(_enemy.TargetStats, _enemy.AttackDamage, _enemy.Id);
        }
    }
}


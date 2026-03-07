using Fusion;
using UnityEngine;

public class EnemyDamageService
{
    public bool ApplyDamage(EnemyController enemyController, NetworkId enemyId, int amount, NetworkId? attackerId = null)
    {
        if (enemyController == null || amount <= 0)
            return false;

        if (!enemyController.HasStateAuthority)
            return false;

        if (!enemyController.TryGetEnemyState(enemyId, out var state))
            return false;

        if (state.HP <= 0)
        {
            enemyController.OnEnemyDeath(enemyId, attackerId);
            return false;
        }

        int nextHp = Mathf.Max(0, state.HP - amount);
        if (nextHp <= 0)
        {
            enemyController.OnEnemyDeath(enemyId, attackerId);
            return true;
        }

        state.HP = nextHp;
        enemyController.SetEnemyState(enemyId, state);
        return true;
    }
}

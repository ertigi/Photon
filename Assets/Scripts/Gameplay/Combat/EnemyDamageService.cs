using Fusion;

public class EnemyDamageService
{
    public bool ApplyDamage(EnemyController enemyController, NetworkId enemyId, int amount, NetworkId? attackerId = null)
    {
        if (enemyController == null || amount <= 0)
            return false;

        if (!enemyController.HasStateAuthority)
            return false;

        return enemyController.TryApplyDamage(enemyId, amount, attackerId);
    }
}

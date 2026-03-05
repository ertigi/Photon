using Fusion;

public class PlayerDamageService
{
    public bool ApplyDamage(PlayerStatsNetwork target, int amount, NetworkId? attackerId = null)
    {
        if (target == null || amount <= 0)
            return false;

        if (target.Object == null || !target.Object.IsValid)
            return false;

        if (!target.HasStateAuthority || target.IsDead)
            return false;

        target.ApplyDamage(amount);
        return true;
    }
}

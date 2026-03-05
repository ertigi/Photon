public class LootApplyService
{
    public bool TryApply(PlayerStatsNetwork target, LootType lootType, int value)
    {
        if (target == null || value <= 0)
            return false;

        if (target.Object == null || !target.Object.IsValid)
            return false;

        if (!target.HasStateAuthority || target.IsDead)
            return false;

        switch (lootType)
        {
            case LootType.Potion:
                target.Heal(value);
                return true;
            case LootType.XpCrystal:
                target.AddXp(value);
                return true;
            default:
                return false;
        }
    }
}

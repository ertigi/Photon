using Fusion;
using UnityEngine;

public class EnemyLootDropService
{
    private readonly PrefabsConfig _prefabsConfig;
    private readonly LootConfig _lootConfig;

    public EnemyLootDropService(PrefabsConfig prefabsConfig, LootConfig lootConfig)
    {
        _prefabsConfig = prefabsConfig;
        _lootConfig = lootConfig;
    }

    public bool TrySpawnLoot(NetworkRunner runner, Vector3 enemyDeathPosition, NetworkId? killerId = null)
    {
        if (runner == null || !runner.IsRunning || !runner.IsServer)
            return false;

        if (!TryRollDrop(out var lootType, out var lootValue))
            return false;

        NetworkPrefabRef lootPrefab = GetPrefab(lootType);
        Vector3 spawnPosition = enemyDeathPosition + Vector3.up * _lootConfig.SpawnYOffset;

        NetworkObject lootObject = runner.Spawn(lootPrefab, spawnPosition, Quaternion.identity);
        if (lootObject == null)
            return false;

        var pickup = lootObject.GetComponent<LootPickupNetwork>();
        if (pickup == null || !pickup.HasStateAuthority)
        {
            if (lootObject.IsValid)
                runner.Despawn(lootObject);

            return false;
        }

        pickup.Initialize(lootType, lootValue, _lootConfig.PickupRadius, spawnPosition);
        return true;
    }

    private bool TryRollDrop(out LootType lootType, out int lootValue)
    {
        lootType = default;
        lootValue = 0;

        float potionChance = Mathf.Clamp01(_lootConfig.PotionDropChance);
        float xpChance = Mathf.Clamp01(_lootConfig.XpCrystalDropChance);

        float roll = Random.value;

        if (roll < potionChance)
        {
            lootType = LootType.Potion;
            lootValue = Mathf.Max(1, _lootConfig.PotionHealValue);
            return true;
        }

        if (roll < potionChance + xpChance)
        {
            lootType = LootType.XpCrystal;
            lootValue = Mathf.Max(1, _lootConfig.XpCrystalValue);
            return true;
        }

        return false;
    }

    private NetworkPrefabRef GetPrefab(LootType lootType)
    {
        return lootType == LootType.Potion
            ? _prefabsConfig.NetworkPotionLootPrefab
            : _prefabsConfig.NetworkXpCrystalLootPrefab;
    }
}

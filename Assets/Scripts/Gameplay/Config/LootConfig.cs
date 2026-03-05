using UnityEngine;

[CreateAssetMenu(fileName = "LootConfig", menuName = "Gameplay/Loot Config")]
public class LootConfig : ScriptableObject
{
    [field: SerializeField, Range(0f, 1f)] public float PotionDropChance { get; private set; } = 0.4f;
    [field: SerializeField, Range(0f, 1f)] public float XpCrystalDropChance { get; private set; } = 0.5f;

    [field: SerializeField, Min(1)] public int PotionHealValue { get; private set; } = 25;
    [field: SerializeField, Min(1)] public int XpCrystalValue { get; private set; } = 20;

    [field: SerializeField, Min(0.2f)] public float PickupRadius { get; private set; } = 1.35f;
    [field: SerializeField] public float SpawnYOffset { get; private set; } = 0.35f;
}

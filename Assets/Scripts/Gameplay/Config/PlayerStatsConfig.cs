using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "Gameplay/Player Stats Config")]
public class PlayerStatsConfig : ScriptableObject
{
    [Header("Base Stats")]
    [field: SerializeField, Min(1)] public int BaseMaxHp { get; private set; } = 100;
    [field: SerializeField, Min(1)] public int BaseDamage { get; private set; } = 10;
    [field: SerializeField, Min(0.1f)] public float BaseAttackRate { get; private set; } = 1f;
    [field: SerializeField, Min(0.1f)] public float BaseAttackRadius { get; private set; } = 2.5f;
    [field: SerializeField, Min(0.1f)] public float BaseMoveSpeedMultiplier { get; private set; } = 1f;
}

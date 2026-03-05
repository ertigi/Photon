using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ProgressionConfig", menuName = "Gameplay/Progression Config")]
public class ProgressionConfig : ScriptableObject
{
    [Serializable]
    public struct LevelBonusWeight
    {
        public LevelBonusType BonusType;
        [Min(0f)] public float Weight;
    }

    [Header("XP Curve (formula fallback)")]
    [field: SerializeField, Min(1)] public int BaseXpPerLevel { get; private set; } = 100;
    [field: SerializeField, Min(1f)] public float XpGrowthPerLevel { get; private set; } = 1.2f;

    [Header("Optional explicit thresholds by level index (1-based)")]
    [SerializeField] private int[] _xpThresholdByLevel;

    [Header("Random level-up bonus weights")]
    [SerializeField] private LevelBonusWeight[] _bonusWeights =
    {
        new() { BonusType = LevelBonusType.AttackSpeed, Weight = 1f },
        new() { BonusType = LevelBonusType.Damage, Weight = 1f },
        new() { BonusType = LevelBonusType.MaxHp, Weight = 1f },
        new() { BonusType = LevelBonusType.MoveSpeed, Weight = 1f }
    };

    [Header("Bonus Magnitudes")]
    [field: SerializeField, Min(1)] public int DamageBonusStep { get; private set; } = 2;
    [field: SerializeField, Min(1)] public int MaxHpBonusStep { get; private set; } = 20;
    [field: SerializeField, Min(0.01f)] public float AttackRateBonusStep { get; private set; } = 0.15f;
    [field: SerializeField, Min(0.01f)] public float MoveSpeedBonusStep { get; private set; } = 0.1f;

    public int[] XpThresholdByLevel => _xpThresholdByLevel;
    public LevelBonusWeight[] BonusWeights => _bonusWeights;
}

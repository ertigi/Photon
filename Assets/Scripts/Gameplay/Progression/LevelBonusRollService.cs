using System;
using UnityEngine;

public class LevelBonusRollService
{
    private readonly ProgressionConfig _progressionConfig;

    public LevelBonusRollService(ProgressionConfig progressionConfig)
    {
        _progressionConfig = progressionConfig;
    }

    public LevelBonusType Roll(bool hasStateAuthority)
    {
        if (!hasStateAuthority)
            throw new InvalidOperationException("LevelBonusRollService.Roll must be called on StateAuthority.");

        var weights = _progressionConfig.BonusWeights;
        if (weights == null || weights.Length == 0)
            return LevelBonusType.Damage;

        float totalWeight = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            if (weights[i].Weight > 0f)
                totalWeight += weights[i].Weight;
        }

        if (totalWeight <= 0f)
            return weights[0].BonusType;

        float pick = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            float weight = Mathf.Max(0f, weights[i].Weight);
            if (weight <= 0f)
                continue;

            cumulative += weight;
            if (pick <= cumulative)
                return weights[i].BonusType;
        }

        return weights[weights.Length - 1].BonusType;
    }
}

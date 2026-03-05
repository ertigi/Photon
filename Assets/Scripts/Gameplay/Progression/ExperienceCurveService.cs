using UnityEngine;

public class ExperienceCurveService
{
    private readonly ProgressionConfig _progressionConfig;

    public ExperienceCurveService(ProgressionConfig progressionConfig)
    {
        _progressionConfig = progressionConfig;
    }

    public int GetXpForNextLevel(int currentLevel)
    {
        int level = Mathf.Max(1, currentLevel);
        int index = level - 1;

        var explicitThresholds = _progressionConfig.XpThresholdByLevel;
        if (explicitThresholds != null && index < explicitThresholds.Length)
        {
            int explicitValue = explicitThresholds[index];
            if (explicitValue > 0)
                return explicitValue;
        }

        float threshold = _progressionConfig.BaseXpPerLevel * Mathf.Pow(_progressionConfig.XpGrowthPerLevel, index);
        return Mathf.Max(1, Mathf.RoundToInt(threshold));
    }

    // Backward-compatible alias.
    public int GetXpThreshold(int currentLevel)
    {
        return GetXpForNextLevel(currentLevel);
    }
}

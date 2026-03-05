using System;
using Fusion;
using UnityEngine;
using Zenject;

public class PlayerStatsNetwork : NetworkBehaviour
{
    private PlayerStatsConfig _playerStatsConfig;
    private ProgressionConfig _progressionConfig;
    private ExperienceCurveService _experienceCurveService;
    private LevelBonusRollService _levelBonusRollService;

    [Networked] public int HP { get; private set; }
    [Networked] public int MaxHP { get; private set; }
    [Networked] public int XP { get; private set; }
    [Networked] public int Level { get; private set; }
    [Networked] public int Damage { get; private set; }
    [Networked] public float AttackRate { get; private set; }
    [Networked] public float AttackRadius { get; private set; }
    [Networked] public float MoveSpeedMultiplier { get; private set; }
    [Networked] public bool IsDead { get; private set; }

    public event Action<PlayerStatsNetwork> ServerDied;
    public event Action OnChangeHP;

    [Inject]
    public void Construct(
        PlayerStatsConfig playerStatsConfig,
        ProgressionConfig progressionConfig,
        ExperienceCurveService experienceCurveService,
        LevelBonusRollService levelBonusRollService)
    {
        _playerStatsConfig = playerStatsConfig;
        _progressionConfig = progressionConfig;
        _experienceCurveService = experienceCurveService;
        _levelBonusRollService = levelBonusRollService;
    }

    public override void Spawned()
    {
        if (!HasStateAuthority)
            return;

        InitializeDefaults();
    }

    public void ApplyDamage(int value)
    {
        if (!HasStateAuthority || IsDead || value <= 0)
            return;

        HP = Mathf.Max(0, HP - value);

        if (HP <= 0)
            MarkDead();

        OnChangeHP?.Invoke();
    }

    public void Heal(int value)
    {
        if (!HasStateAuthority || IsDead || value <= 0f)
            return;
            
        HP = Mathf.Clamp(HP + value, 0, MaxHP);

        OnChangeHP?.Invoke();
    }
    public void AddXp(int value)
    {
        if (!HasStateAuthority || IsDead || value <= 0)
            return;

        XP += value;
        ProcessLevelUps();
    }

    public void ApplyBonus(LevelBonusType bonusType)
    {
        if (!HasStateAuthority || IsDead)
            return;

        switch (bonusType)
        {
            case LevelBonusType.AttackSpeed:
                AttackRate = Mathf.Max(0.1f, AttackRate + _progressionConfig.AttackRateBonusStep);
                break;
            case LevelBonusType.Damage:
                Damage += Mathf.Max(1, _progressionConfig.DamageBonusStep);
                break;
            case LevelBonusType.MaxHp:
                MaxHP += Mathf.Max(1, _progressionConfig.MaxHpBonusStep);
                break;
            case LevelBonusType.MoveSpeed:
                MoveSpeedMultiplier = Mathf.Max(0.1f, MoveSpeedMultiplier + _progressionConfig.MoveSpeedBonusStep);
                break;
        }
    }

    public void ForceKill()
    {
        if (!HasStateAuthority || IsDead)
            return;

        MarkDead();
    }

    private void InitializeDefaults()
    {
        Level = Mathf.Max(1, Level);
        MaxHP = Mathf.Max(1, _playerStatsConfig.BaseMaxHp);
        HP = MaxHP;
        XP = 0;
        Damage = Mathf.Max(1, _playerStatsConfig.BaseDamage);
        AttackRate = Mathf.Max(0.1f, _playerStatsConfig.BaseAttackRate);
        AttackRadius = Mathf.Max(0.1f, _playerStatsConfig.BaseAttackRadius);
        MoveSpeedMultiplier = Mathf.Max(0.1f, _playerStatsConfig.BaseMoveSpeedMultiplier);
        IsDead = false;
    }

    private void ProcessLevelUps()
    {
        int threshold = _experienceCurveService.GetXpForNextLevel(Level);

        while (XP >= threshold)
        {
            XP -= threshold;
            Level += 1;

            LevelBonusType randomBonus = _levelBonusRollService.Roll(HasStateAuthority);
            ApplyBonus(randomBonus);

            HP = MaxHP;
            threshold = _experienceCurveService.GetXpForNextLevel(Level);
        }
        
        OnChangeHP?.Invoke();
    }

    private void MarkDead()
    {
        if (IsDead)
            return;

        IsDead = true;
        HP = 0;
        ServerDied?.Invoke(this);
    }
}

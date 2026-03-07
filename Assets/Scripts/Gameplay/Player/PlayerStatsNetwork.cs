using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Zenject;

public class PlayerStatsNetwork : NetworkBehaviour
{
    public interface IRenderChangeListener
    {
        void HandleHealthChanged(int hp, int maxHp, bool isDead);
        void HandleProgressChanged(int xp, int level);
        void HandleDeathChanged(bool isDead);
    }

    private PlayerStatsConfig _playerStatsConfig;
    private ProgressionConfig _progressionConfig;
    private ExperienceCurveService _experienceCurveService;
    private LevelBonusRollService _levelBonusRollService;

    private readonly List<IRenderChangeListener> _renderListeners = new(2);
    private readonly List<Action<PlayerStatsNetwork>> _serverDeathCallbacks = new(1);

    private ChangeDetector _healthChangeDetector;
    private ChangeDetector _progressChangeDetector;
    private bool _renderDetectorsInitialized;

    [Networked] public int HP { get; private set; }
    [Networked] public int MaxHP { get; private set; }
    [Networked] public int XP { get; private set; }
    [Networked] public int Level { get; private set; }
    [Networked] public int Damage { get; private set; }
    [Networked] public float AttackRate { get; private set; }
    [Networked] public float AttackRadius { get; private set; }
    [Networked] public float MoveSpeedMultiplier { get; private set; }
    [Networked] public bool IsDead { get; private set; }

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
        _healthChangeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotFrom, copyInitial: true);
        _progressChangeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotFrom, copyInitial: true);
        _renderDetectorsInitialized = true;

        if (!HasStateAuthority)
        {
            PushInitialStateToAllListeners();
            return;
        }

        InitializeDefaults();
        PushInitialStateToAllListeners();
    }

    public override void Render()
    {
        if (!_renderDetectorsInitialized || !StateBufferIsValid)
            return;

        bool deathChanged;
        if (TryDetectHealthChanges(out deathChanged))
            HandleHealthChanged();

        if (TryDetectProgressChanges())
            HandleProgressChanged();

        if (deathChanged)
            HandleDeathChanged();
    }

    public void RegisterRenderListener(IRenderChangeListener listener, bool pushInitialState = true)
    {
        if (listener == null)
            return;

        if (_renderListeners.Contains(listener))
            return;

        _renderListeners.Add(listener);

        if (!pushInitialState || !StateBufferIsValid)
            return;

        listener.HandleHealthChanged(HP, MaxHP, IsDead);
        listener.HandleProgressChanged(XP, Level);
        listener.HandleDeathChanged(IsDead);
    }

    public void UnregisterRenderListener(IRenderChangeListener listener)
    {
        if (listener == null)
            return;

        _renderListeners.Remove(listener);
    }

    public void RegisterServerDeathCallback(Action<PlayerStatsNetwork> callback)
    {
        if (callback == null)
            return;

        if (_serverDeathCallbacks.Contains(callback))
            return;

        _serverDeathCallbacks.Add(callback);
    }

    public void UnregisterServerDeathCallback(Action<PlayerStatsNetwork> callback)
    {
        if (callback == null)
            return;

        _serverDeathCallbacks.Remove(callback);
    }

    public void ApplyDamage(int value)
    {
        if (!HasStateAuthority || IsDead || value <= 0)
            return;

        HP = Mathf.Max(0, HP - value);
        EnsureStateInvariants();

        if (HP <= 0)
        {
            MarkDead();
            return;
        }
    }

    public void Heal(int value)
    {
        if (!HasStateAuthority || IsDead || value <= 0)
            return;

        HP = Mathf.Clamp(HP + value, 0, MaxHP);
        EnsureStateInvariants();
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
        ApplyBonusInternal(bonusType);
    }

    public void ForceKill()
    {
        if (!HasStateAuthority || IsDead)
            return;

        MarkDead();
    }

    private void ApplyBonusInternal(LevelBonusType bonusType)
    {
        if (!HasStateAuthority || IsDead)
            return;

        switch (bonusType)
        {
            case LevelBonusType.AttackSpeed:
                AttackRate = AttackRate + _progressionConfig.AttackRateBonusStep;
                break;
            case LevelBonusType.Damage:
                Damage += _progressionConfig.DamageBonusStep;
                break;
            case LevelBonusType.MaxHp:
                MaxHP += _progressionConfig.MaxHpBonusStep;
                break;
            case LevelBonusType.MoveSpeed:
                MoveSpeedMultiplier = MoveSpeedMultiplier + _progressionConfig.MoveSpeedBonusStep;
                break;
        }

        EnsureStateInvariants();
    }

    private void InitializeDefaults()
    {
        Level = 1;
        MaxHP = _playerStatsConfig.BaseMaxHp;
        HP = MaxHP;
        XP = 0;
        Damage = _playerStatsConfig.BaseDamage;
        AttackRate = _playerStatsConfig.BaseAttackRate;
        AttackRadius = _playerStatsConfig.BaseAttackRadius;
        MoveSpeedMultiplier = _playerStatsConfig.BaseMoveSpeedMultiplier;
        IsDead = false;

        EnsureStateInvariants();
    }

    private void ProcessLevelUps()
    {
        if (!HasStateAuthority || IsDead)
            return;

        while (true)
        {
            int threshold = Mathf.Max(1, _experienceCurveService.GetXpForNextLevel(Level));
            if (XP < threshold)
                break;

            LevelUp(threshold);
        }
    }

    private void LevelUp(int xpThreshold)
    {
        if (!HasStateAuthority || IsDead)
            return;

        int safeThreshold = Mathf.Max(1, xpThreshold);
        if (XP < safeThreshold)
            return;

        XP -= safeThreshold;
        Level = Mathf.Max(1, Level + 1);

        LevelBonusType randomBonus = _levelBonusRollService.Roll(HasStateAuthority);
        ApplyBonusInternal(randomBonus);

        HP = MaxHP;
        EnsureStateInvariants();
    }

    private void MarkDead()
    {
        if (!HasStateAuthority)
            return;

        if (IsDead)
            return;

        IsDead = true;
        HP = 0;
        EnsureStateInvariants();
        InvokeServerDeathCallbacks();
    }

    private void EnsureStateInvariants()
    {
        MaxHP = Mathf.Max(1, MaxHP);

        if (IsDead)
        {
            HP = 0;
            return;
        }

        HP = Mathf.Clamp(HP, 0, MaxHP);
    }

    private void HandleHealthChanged()
    {
        if (!StateBufferIsValid)
            return;

        for (int i = 0; i < _renderListeners.Count; i++)
        {
            _renderListeners[i].HandleHealthChanged(HP, MaxHP, IsDead);
        }
    }

    private void HandleProgressChanged()
    {
        if (!StateBufferIsValid)
            return;

        for (int i = 0; i < _renderListeners.Count; i++)
        {
            _renderListeners[i].HandleProgressChanged(XP, Level);
        }
    }

    private void HandleDeathChanged()
    {
        if (!StateBufferIsValid)
            return;

        for (int i = 0; i < _renderListeners.Count; i++)
        {
            _renderListeners[i].HandleDeathChanged(IsDead);
        }
    }

    private void PushInitialStateToAllListeners()
    {
        if (!StateBufferIsValid)
            return;

        HandleHealthChanged();
        HandleProgressChanged();
        HandleDeathChanged();
    }

    private void InvokeServerDeathCallbacks()
    {
        for (int i = 0; i < _serverDeathCallbacks.Count; i++)
        {
            _serverDeathCallbacks[i]?.Invoke(this);
        }
    }

    private bool TryDetectHealthChanges(out bool deathChanged)
    {
        deathChanged = false;
        bool healthChanged = false;

        foreach (var changedProperty in _healthChangeDetector.DetectChanges(this))
        {
            switch (changedProperty)
            {
                case nameof(HP):
                case nameof(MaxHP):
                    healthChanged = true;
                    break;
                case nameof(IsDead):
                    healthChanged = true;
                    deathChanged = true;
                    break;
            }
        }

        return healthChanged;
    }

    private bool TryDetectProgressChanges()
    {
        bool progressChanged = false;

        foreach (var changedProperty in _progressChangeDetector.DetectChanges(this))
        {
            switch (changedProperty)
            {
                case nameof(XP):
                case nameof(Level):
                    progressChanged = true;
                    break;
            }
        }

        return progressChanged;
    }
}

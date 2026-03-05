using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

public class EnemyController : NetworkBehaviour
{
    private NetworkPrefabRef _enemyPrefab;
    private EnemySpawnConfig _enemySpawnConfig;
    private EnemySpawnService _enemySpawnService;
    private EnemyTargetingService _enemyTargetingService;
    private EnemyLootDropService _enemyLootDropService;
    private PlayerDamageService _playerDamageService;
    private EnemyDamageService _enemyDamageService;
    private CancellationTokenSource _spawnLoopCts;

    [Networked, Capacity(128)]
    private NetworkDictionary<NetworkId, EnemyNetworkData> EnemyDatas => default;

    private readonly Dictionary<NetworkId, Enemy> _runtimes = new();

    [Inject]
    private void Construct(
        PrefabsConfig prefabsConfig,
        EnemySpawnConfig enemySpawnConfig,
        EnemySpawnService enemySpawnService,
        EnemyTargetingService enemyTargetingService,
        EnemyLootDropService enemyLootDropService,
        PlayerDamageService playerDamageService,
        EnemyDamageService enemyDamageService)
    {
        _enemyPrefab = prefabsConfig.NetworkEnemyPrefab;
        _enemySpawnConfig = enemySpawnConfig;
        _enemySpawnService = enemySpawnService;
        _enemyTargetingService = enemyTargetingService;
        _enemyLootDropService = enemyLootDropService;
        _playerDamageService = playerDamageService;
        _enemyDamageService = enemyDamageService;
    }

    public override void Spawned()
    {
        if (!HasStateAuthority)
            return;

        StartSpawnLoop();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        var deadEnemies = new List<NetworkId>();

        foreach (var runtimeEntry in _runtimes)
        {
            NetworkId enemyId = runtimeEntry.Key;
            Enemy runtime = runtimeEntry.Value;

            if (EnemyDatas.TryGet(runtime.Id, out var state))
            {
                if (state.HP <= 0)
                {
                    deadEnemies.Add(enemyId);
                    continue;
                }
            }

            runtime.StateMachine.Tick(Runner.DeltaTime);
        }

        for (int i = 0; i < deadEnemies.Count; i++)
        {
            HandleEnemyDeath(deadEnemies[i], null);
        }

    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        StopSpawnLoop();
    }

    private void OnDestroy()
    {
        StopSpawnLoop();
    }

    public void SpawnEnemy(Vector3 position)
    {
        var obj = Runner.Spawn(_enemyPrefab, position, Quaternion.identity);
        var view = obj.GetComponent<EnemyView>();
        view.SetOwnerController(this);
        var id = view.Object.Id;

        EnemyDatas.Add(id, new EnemyNetworkData { HP = 100 });

        var runtime = new Enemy(id, view, _enemyTargetingService, _playerDamageService);

        _runtimes.Add(id, runtime);
    }

    public void ApplyDamage(NetworkId id, int damage)
    {
        _enemyDamageService.ApplyDamage(this, id, damage);
    }

    public bool TryApplyDamage(NetworkId id, int damage, NetworkId? attackerId = null)
    {
        if (!HasStateAuthority || damage <= 0)
            return false;

        if (!EnemyDatas.TryGet(id, out var state))
            return false;

        if (state.HP <= 0)
        {
            HandleEnemyDeath(id, attackerId);
            return false;
        }

        int nextHp = Mathf.Max(0, state.HP - damage);
        state.HP = nextHp;

        if (nextHp <= 0)
        {
            HandleEnemyDeath(id, attackerId);
            return true;
        }

        EnemyDatas.Set(id, state);
        return true;
    }

    private void HandleEnemyDeath(NetworkId id, NetworkId? killerId)
    {
        if (!HasStateAuthority)
            return;

        if (_runtimes.TryGetValue(id, out var runtime))
        {
            Vector3 deathPosition = runtime.View != null ? runtime.View.transform.position : Vector3.zero;
            _enemyLootDropService.TrySpawnLoot(Runner, deathPosition, killerId);

            if (runtime.View != null && runtime.View.Object != null && runtime.View.Object.IsValid)
                Runner.Despawn(runtime.View.Object);

            _runtimes.Remove(id);
        }

        EnemyDatas.Remove(id);
    }

    private void StartSpawnLoop()
    {
        StopSpawnLoop();

        _spawnLoopCts = new CancellationTokenSource();
        SpawnLoopAsync(_spawnLoopCts.Token).Forget();
    }

    private void StopSpawnLoop()
    {
        if (_spawnLoopCts == null)
            return;

        _spawnLoopCts.Cancel();
        _spawnLoopCts.Dispose();
        _spawnLoopCts = null;
    }

    private async UniTaskVoid SpawnLoopAsync(CancellationToken cancellationToken)
    {
        TrySpawnByLimit();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                float interval = Mathf.Max(0.1f, _enemySpawnConfig.Interval);
                await UniTask.Delay(
                    TimeSpan.FromSeconds(interval),
                    DelayType.DeltaTime,
                    PlayerLoopTiming.FixedUpdate,
                    cancellationToken);

                if (cancellationToken.IsCancellationRequested || !HasStateAuthority || Runner == null || !Runner.IsRunning)
                    return;

                TrySpawnByLimit();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void TrySpawnByLimit()
    {
        int maxEnemies = Mathf.Max(0, _enemySpawnConfig.MaxEnemies);
        if (maxEnemies <= 0)
            return;

        if (_runtimes.Count >= maxEnemies)
            return;

        if (!_enemySpawnService.TryGetSpawnPoint(Runner, out var spawnPosition))
            return;

        SpawnEnemy(spawnPosition);
    }
}

public class Enemy
{
    private readonly EnemyTargetingService _enemyTargetingService;
    private readonly PlayerDamageService _playerDamageService;

    public NetworkId Id;
    public EnemyView View;
    public Player TargetPlayer;
    public PlayerStatsNetwork TargetStats;
    public Transform Target => TargetPlayer != null ? TargetPlayer.transform : null;

    public float MoveSpeed = 3f;
    public float RotationSpeed = 180f;
    public float AggroRadius = 12f;
    public float AttackDistance = 2f;

    public float AttackCooldown = 1.5f;
    public float AttackTimer;
    public int AttackDamage = 5;
    public PlayerDamageService PlayerDamageService => _playerDamageService;

    public StateMachine StateMachine;

    public Enemy(
        NetworkId id,
        EnemyView view,
        EnemyTargetingService enemyTargetingService,
        PlayerDamageService playerDamageService)
    {
        Id = id;
        View = view;
        _enemyTargetingService = enemyTargetingService;
        _playerDamageService = playerDamageService;
        StateMachine = new StateMachine(
            new EnemyIdleState(this),
            new EnemyChaseState(this),
            new EnemyAttackState(this)
        );

        StateMachine.SetState<EnemyIdleState>();
    }

    
    public void AssignTarget()
    {
        if (_enemyTargetingService.TryGetClosestAlivePlayer(
                View.transform.position,
                AggroRadius,
                out var player,
                out var playerStats))
        {
            TargetPlayer = player;
            TargetStats = playerStats;
            return;
        }

        ClearTarget();
    }

    public bool HasAliveTarget()
    {
        if (TargetPlayer == null || TargetStats == null)
            return false;

        if (TargetPlayer.Object == null || !TargetPlayer.Object.IsValid)
            return false;

        return !TargetStats.IsDead;
    }

    public void ClearTarget()
    {
        TargetPlayer = null;
        TargetStats = null;
    }

    public void TryDamagePlayer()
    {
        if (TargetStats == null)
            return;

        _playerDamageService.ApplyDamage(TargetStats, AttackDamage, Id);
    }
}

public struct EnemyNetworkData : INetworkStruct
{
    public int HP;
}

public class EnemyTargetingService
{
    private readonly PlayerRuntimeRegistry _playerRuntimeRegistry;

    public EnemyTargetingService(PlayerRuntimeRegistry playerRuntimeRegistry)
    {
        _playerRuntimeRegistry = playerRuntimeRegistry;
    }

    public bool TryGetClosestAlivePlayer(
        Vector3 enemyPosition,
        float maxDistance,
        out Player player,
        out PlayerStatsNetwork playerStats)
    {
        player = null;
        playerStats = null;

        if (maxDistance <= 0f)
            return false;

        float bestDistanceSqr = maxDistance * maxDistance;
        bool found = false;

        foreach (var candidate in _playerRuntimeRegistry.GetAll())
        {
            if (!TryGetAliveCandidate(candidate, out var candidateStats))
                continue;

            float distanceSqr = (candidate.transform.position - enemyPosition).sqrMagnitude;
            if (distanceSqr > bestDistanceSqr)
                continue;

            bestDistanceSqr = distanceSqr;
            player = candidate;
            playerStats = candidateStats;
            found = true;
        }

        return found;
    }

    private static bool TryGetAliveCandidate(Player candidate, out PlayerStatsNetwork playerStats)
    {
        playerStats = null;

        if (candidate == null || candidate.Object == null || !candidate.Object.IsValid)
            return false;

        playerStats = candidate.GetComponent<PlayerStatsNetwork>();
        if (playerStats == null || playerStats.IsDead)
            return false;

        return true;
    }
}

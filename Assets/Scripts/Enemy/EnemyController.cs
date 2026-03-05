using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Zenject;

public class EnemyController : NetworkBehaviour
{
    private NetworkPrefabRef _enemyPrefab;
    private EnemyTargetingService _enemyTargetingService;

    [Networked, Capacity(128)]
    private NetworkDictionary<NetworkId, EnemyNetworkData> EnemyDatas => default;

    private readonly Dictionary<NetworkId, Enemy> _runtimes = new();

    [Inject]
    private void Construct(PrefabsConfig prefabsConfig, EnemyTargetingService enemyTargetingService)
    {
        _enemyPrefab = prefabsConfig.NetworkEnemyPrefab;
        _enemyTargetingService = enemyTargetingService;
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
            SpawnEnemy(Vector3.zero);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        foreach (var runtime in _runtimes.Values)
        {
            if (EnemyDatas.TryGet(runtime.Id, out var state))
            {
                if (state.HP <= 0)
                {
                    Runner.Despawn(runtime.View.Object);
                    continue;
                }
            }

            runtime.StateMachine.Tick(Runner.DeltaTime);
        }
    }

    public void SpawnEnemy(Vector3 position)
    {
        var obj = Runner.Spawn(_enemyPrefab, position, Quaternion.identity);
        var view = obj.GetComponent<EnemyView>();
        var id = view.Object.Id;

        EnemyDatas.Add(id, new EnemyNetworkData { HP = 100 });

        var runtime = new Enemy(id, view, _enemyTargetingService);

        _runtimes.Add(id, runtime);
    }

    public void ApplyDamage(NetworkId id, int damage)
    {
        if (!HasStateAuthority)
            return;

        if (!EnemyDatas.TryGet(id, out var state))
            return;

        state.HP -= damage;
        EnemyDatas.Set(id, state);
    }
}

public class Enemy
{
    private readonly EnemyTargetingService _enemyTargetingService;

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

    public StateMachine StateMachine;

    public Enemy(NetworkId id, EnemyView view, EnemyTargetingService enemyTargetingService)
    {
        Id = id;
        View = view;
        _enemyTargetingService = enemyTargetingService;
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


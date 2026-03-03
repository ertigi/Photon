using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Zenject;

public class EnemyController : NetworkBehaviour
{
    private NetworkPrefabRef _enemyPrefab;

    [Networked, Capacity(128)]
    private NetworkDictionary<NetworkId, EnemyNetworkData> EnemyDatas => default;

    private readonly Dictionary<NetworkId, Enemy> _runtimes = new();

    [Inject]
    private void Construct(PrefabsConfig prefabsConfig)
    {
        _enemyPrefab = prefabsConfig.NetworkEnemyPrefab;
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

        var runtime = new Enemy(id, view);

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
    public NetworkId Id;
    public EnemyView View;
    public Transform Target;

    public float MoveSpeed = 3f;
    public float RotationSpeed = 180f;
    public float AggroRadius = 12f;
    public float AttackDistance = 2f;

    public float AttackCooldown = 1.5f;
    public float AttackTimer;

    public StateMachine StateMachine;

    public Enemy(NetworkId id, EnemyView view)
    {
        Id = id;
        View = view;
        StateMachine = new StateMachine(
            new EnemyIdleState(this),
            new EnemyChaseState(this),
            new EnemyAttackState(this)
        );

        StateMachine.SetState<EnemyIdleState>();
    }

    
    public void AssignTarget(Enemy enemy)
    {
        Player[] players = Object.FindObjectsOfType<Player>();

        float minDist = float.MaxValue;
        Transform nearest = null;

        foreach (var p in players)
        {
            float dist = Vector3.Distance(
                enemy.View.transform.position,
                p.transform.position);

            if (dist < enemy.AggroRadius && dist < minDist)
            {
                minDist = dist;
                nearest = p.transform;
            }
        }

        enemy.Target = nearest;
    }

    public void TryDamagePlayer()
    {

    }
}

public struct EnemyNetworkData : INetworkStruct
{
    public int HP;
}


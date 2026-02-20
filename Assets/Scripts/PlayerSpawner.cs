using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class PlayerSpawner
{
    private Dictionary<PlayerRef, NetworkObject> _spawned = new();
    private NetworkPrefabRef _playerPrefab;
    private LocalPlayerRegistry _localPlayerRegistry;

    public PlayerSpawner(LocalPlayerRegistry localPlayerRegistry, PrefabsConfig prefabsConfig)
    {
        _localPlayerRegistry = localPlayerRegistry;
        _playerPrefab = prefabsConfig.PlayerPrefab;
    }

    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        Vector3 pos = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));

        NetworkObject obj = runner.Spawn(_playerPrefab, pos, Quaternion.identity, player);

        obj.GetComponent<Player>().Init(_localPlayerRegistry);

        _spawned[player] = obj;
    }

    public void DespawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (_spawned.TryGetValue(player, out var obj) && obj != null)
        {
            runner.Despawn(obj);
        }

        _spawned.Remove(player);
    }
}
using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner
{
    private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawned = new();

    public PlayerSpawner(NetworkPrefabRef playerPrefab)
    {
        _playerPrefab = playerPrefab;
    }

    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        Vector3 pos = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));

        NetworkObject obj = runner.Spawn(_playerPrefab, pos, Quaternion.identity, player);

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
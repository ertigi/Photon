using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerRuntimeRegistry
{
    private readonly Dictionary<PlayerRef, Player> _playersByRef = new();
    private readonly Dictionary<NetworkId, Player> _playersById = new();

    public void Register(Player player)
    {
        if (player == null || player.Object == null)
            return;

        _playersByRef[player.Object.InputAuthority] = player;
        _playersById[player.Object.Id] = player;
    }

    public void Unregister(Player player)
    {
        if (player == null)
            return;

        if (player.Object != null)
        {
            _playersByRef.Remove(player.Object.InputAuthority);
            _playersById.Remove(player.Object.Id);
        }

        RemoveByReference(player);
    }

    public bool TryGet(PlayerRef playerRef, out Player player)
    {
        return _playersByRef.TryGetValue(playerRef, out player);
    }

    public bool TryGet(NetworkId networkId, out Player player)
    {
        return _playersById.TryGetValue(networkId, out player);
    }

    public IEnumerable<Player> GetAll()
    {
        return _playersByRef.Values;
    }

    public bool TryGetClosest(Vector3 position, out Player closest, float maxDistance = float.PositiveInfinity)
    {
        closest = null;
        float bestDistanceSqr = maxDistance * maxDistance;
        bool found = false;

        foreach (var player in _playersByRef.Values)
        {
            if (player == null)
                continue;

            float distanceSqr = (player.transform.position - position).sqrMagnitude;
            if (distanceSqr >= bestDistanceSqr)
                continue;

            bestDistanceSqr = distanceSqr;
            closest = player;
            found = true;
        }

        return found;
    }

    private void RemoveByReference(Player player)
    {
        PlayerRef stalePlayerRef = default;
        bool hasPlayerRef = false;

        foreach (var pair in _playersByRef)
        {
            if (pair.Value != player)
                continue;

            stalePlayerRef = pair.Key;
            hasPlayerRef = true;
            break;
        }

        if (hasPlayerRef)
            _playersByRef.Remove(stalePlayerRef);

        NetworkId staleNetworkId = default;
        bool hasNetworkId = false;

        foreach (var pair in _playersById)
        {
            if (pair.Value != player)
                continue;

            staleNetworkId = pair.Key;
            hasNetworkId = true;
            break;
        }

        if (hasNetworkId)
            _playersById.Remove(staleNetworkId);
    }
}

using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class EnemyRuntimeRegistry
{
    private readonly Dictionary<NetworkId, EnemyView> _enemiesById = new();

    public void Register(EnemyView enemy)
    {
        if (enemy == null || enemy.Object == null)
            return;

        _enemiesById[enemy.Object.Id] = enemy;
    }

    public void Unregister(EnemyView enemy)
    {
        if (enemy == null)
            return;

        if (enemy.Object != null)
            _enemiesById.Remove(enemy.Object.Id);

        RemoveByReference(enemy);
    }

    public bool TryGet(NetworkId networkId, out EnemyView enemy)
    {
        return _enemiesById.TryGetValue(networkId, out enemy);
    }

    public IEnumerable<EnemyView> GetAll()
    {
        return _enemiesById.Values;
    }

    public bool TryGetClosest(Vector3 position, out EnemyView closest, float maxDistance = float.PositiveInfinity)
    {
        closest = null;
        float bestDistanceSqr = maxDistance * maxDistance;
        bool found = false;

        foreach (var enemy in _enemiesById.Values)
        {
            if (enemy == null)
                continue;

            float distanceSqr = (enemy.transform.position - position).sqrMagnitude;
            if (distanceSqr >= bestDistanceSqr)
                continue;

            bestDistanceSqr = distanceSqr;
            closest = enemy;
            found = true;
        }

        return found;
    }

    private void RemoveByReference(EnemyView enemy)
    {
        NetworkId staleNetworkId = default;
        bool hasNetworkId = false;

        foreach (var pair in _enemiesById)
        {
            if (pair.Value != enemy)
                continue;

            staleNetworkId = pair.Key;
            hasNetworkId = true;
            break;
        }

        if (hasNetworkId)
            _enemiesById.Remove(staleNetworkId);
    }
}

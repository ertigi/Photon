using Fusion;
using UnityEngine;

public class EnemySpawnService
{
    private const int MaxPlacementAttempts = 20;

    private readonly EnemySpawnConfig _config;
    private readonly PlayerRuntimeRegistry _playerRuntimeRegistry;

    public EnemySpawnService(EnemySpawnConfig config, PlayerRuntimeRegistry playerRuntimeRegistry)
    {
        _config = config;
        _playerRuntimeRegistry = playerRuntimeRegistry;
    }

    public bool TryGetSpawnPoint(NetworkRunner runner, out Vector3 spawnPoint)
    {
        spawnPoint = Vector3.zero;

        if (runner == null)
            return false;

        Vector3 center = ResolveCenter(runner);
        float radius = Mathf.Max(0.5f, _config.Radius);
        float minDistanceToPlayers = Mathf.Max(0f, _config.MinDistanceToPlayers);

        for (int i = 0; i < MaxPlacementAttempts; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            Vector3 candidate = new Vector3(center.x + randomOffset.x, 0f, center.z + randomOffset.y);

            if (!IsFarEnoughFromPlayers(candidate, minDistanceToPlayers))
                continue;

            spawnPoint = candidate;
            return true;
        }

        return false;
    }

    private Vector3 ResolveCenter(NetworkRunner runner)
    {
        var hostObject = runner.GetPlayerObject(runner.LocalPlayer);
        if (hostObject != null && hostObject.IsValid)
        {
            var position = hostObject.transform.position;
            position.y = 0f;
            return position;
        }

        foreach (var player in _playerRuntimeRegistry.GetAll())
        {
            if (player == null || player.Object == null || !player.Object.IsValid)
                continue;

            var position = player.transform.position;
            position.y = 0f;
            return position;
        }

        return Vector3.zero;
    }

    private bool IsFarEnoughFromPlayers(Vector3 candidate, float minDistanceToPlayers)
    {
        float minDistanceSqr = minDistanceToPlayers * minDistanceToPlayers;

        foreach (var player in _playerRuntimeRegistry.GetAll())
        {
            if (player == null || player.Object == null || !player.Object.IsValid)
                continue;

            float distanceSqr = (player.transform.position - candidate).sqrMagnitude;
            if (distanceSqr < minDistanceSqr)
                return false;
        }

        return true;
    }
}

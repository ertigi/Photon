using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnConfig", menuName = "Gameplay/Enemy Spawn Config")]
public class EnemySpawnConfig : ScriptableObject
{
    [field: SerializeField, Min(0.5f)] public float Radius { get; private set; } = 20f;
    [field: SerializeField, Min(0f)] public float MinDistanceToPlayers { get; private set; } = 8f;
    [field: SerializeField, Min(0.1f)] public float Interval { get; private set; } = 2f;
    [field: SerializeField, Min(0)] public int MaxEnemies { get; private set; } = 20;
}

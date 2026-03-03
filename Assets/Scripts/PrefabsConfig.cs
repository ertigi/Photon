using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabsConfig", menuName = "PrefabsConfig", order = 1)]
public class PrefabsConfig : ScriptableObject
{
    [field: SerializeField] public NetworkPrefabRef NetworkPlayerPrefab { get; private set; }
    [field: SerializeField] public Player PlayerPrefabSource { get; private set; }
    [field: SerializeField] public NetworkPrefabRef NetworkEnemyPrefab { get; private set; }
    [field: SerializeField] public EnemyView EnemyPrefabSource { get; private set; }
}


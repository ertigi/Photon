using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabsConfig", menuName = "PrefabsConfig", order = 1)]
public class PrefabsConfig : ScriptableObject
{
    [field: SerializeField] public NetworkPrefabRef PlayerPrefab { get; private set; }
}
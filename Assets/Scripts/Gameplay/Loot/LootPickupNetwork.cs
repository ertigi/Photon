using Fusion;
using UnityEngine;
using Zenject;

public class LootPickupNetwork : NetworkBehaviour
{
    public class PotionFactory : PlaceholderFactory<LootPickupNetwork> { }
    public class XpCrystalFactory : PlaceholderFactory<LootPickupNetwork> { }

    [SerializeField, Min(0.2f)] private float _defaultPickupRadius = 1.2f;
    [SerializeField] private Color _potionColor = new Color(0.2f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color _xpCrystalColor = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private Renderer _renderer;

    [Networked] public LootType Type { get; private set; }
    [Networked] public int Value { get; private set; }
    [Networked] private float PickupRadius { get; set; }
    [Networked] private NetworkBool IsPickedUp { get; set; }

    private PlayerRuntimeRegistry _playerRuntimeRegistry;
    private LootApplyService _lootApplyService;

    private LootType _lastVisualType;
    private bool _visualInitialized;

    [Inject]
    public void Construct(PlayerRuntimeRegistry playerRuntimeRegistry, LootApplyService lootApplyService)
    {
        _playerRuntimeRegistry = playerRuntimeRegistry;
        _lootApplyService = lootApplyService;
    }

    public override void Spawned()
    {
        if (HasStateAuthority && PickupRadius <= 0f)
            PickupRadius = _defaultPickupRadius;

        ApplyVisual();
    }

    public void Initialize(LootType lootType, int value, float pickupRadius)
    {
        if (!HasStateAuthority || IsPickedUp)
            return;

        Type = lootType;
        Value = Mathf.Max(1, value);
        PickupRadius = Mathf.Max(0.1f, pickupRadius);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !Runner.IsServer || IsPickedUp)
            return;

        float radius = PickupRadius > 0f ? PickupRadius : _defaultPickupRadius;
        float radiusSqr = radius * radius;
        Vector3 selfPosition = transform.position;

        foreach (var player in _playerRuntimeRegistry.GetAll())
        {
            if (!TryGetAlivePlayerStats(player, out var playerStats))
                continue;

            float distanceSqr = (player.transform.position - selfPosition).sqrMagnitude;
            if (distanceSqr > radiusSqr)
                continue;

            if (!_lootApplyService.TryApply(playerStats, Type, Value))
                continue;

            IsPickedUp = true;
            Runner.Despawn(Object);
            return;
        }
    }

    public override void Render()
    {
        ApplyVisual();
    }

    private bool TryGetAlivePlayerStats(Player player, out PlayerStatsNetwork playerStats)
    {
        playerStats = null;

        if (player == null || player.Object == null || !player.Object.IsValid)
            return false;

        playerStats = player.GetComponent<PlayerStatsNetwork>();
        if (playerStats == null || playerStats.IsDead || !playerStats.HasStateAuthority)
            return false;

        return true;
    }

    private void ApplyVisual()
    {
        if (_renderer == null)
            _renderer = GetComponentInChildren<Renderer>();

        if (_renderer == null)
            return;

        if (_visualInitialized && _lastVisualType == Type)
            return;

        _visualInitialized = true;
        _lastVisualType = Type;

        Material material = _renderer.material;
        material.color = Type == LootType.Potion ? _potionColor : _xpCrystalColor;
    }
}

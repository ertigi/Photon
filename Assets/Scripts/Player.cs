using Fusion;
using UnityEngine;
using Zenject;

public class Player : NetworkBehaviour, ILocalPlayerCameraTarget
{
    public class Factory : PlaceholderFactory<Player> { }

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 180f;

    public Transform FollowTarget => transform;
    private LocalPlayerRegistry _registry;
    private PlayerRuntimeRegistry _runtimeRegistry;
    private PlayerStatsNetwork _stats;

    [Inject]
    public void Construct(LocalPlayerRegistry registry, PlayerRuntimeRegistry runtimeRegistry)
    {
        _registry = registry;
        _runtimeRegistry = runtimeRegistry;
    }

    public override void Spawned()
    {
        _runtimeRegistry.Register(this);
        _stats = GetComponent<PlayerStatsNetwork>();

        if (Object.HasInputAuthority)
            _registry.SetLocalPlayer(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _runtimeRegistry.Unregister(this);

        if (Object.HasInputAuthority)
            _registry.ClearLocalPlayer(this);
    }

    private void OnDestroy()
    {
        _runtimeRegistry?.Unregister(this);
        _registry?.ClearLocalPlayer(this);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (GetInput<InputData>(out var input))
        {
            float deltaTime = Runner.DeltaTime;
            Vector3 moveDirection = new Vector3(input.MoveX, 0f, input.MoveY);

            if (moveDirection.sqrMagnitude > 1f)
                moveDirection.Normalize();

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * deltaTime);
                float speedMultiplier = _stats != null ? _stats.MoveSpeedMultiplier : 1f;
                transform.position += moveDirection * (_moveSpeed * speedMultiplier * deltaTime);
            }
        }
    }
}

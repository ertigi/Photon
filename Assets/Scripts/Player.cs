using Fusion;
using UnityEngine;
using Zenject;

public class Player : NetworkBehaviour, ILocalPlayerCameraTarget
{
    public class Factory : PlaceholderFactory<Player> { }

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 180f;
    [SerializeField] private float _proxyLerp = 20f;

    public Transform FollowTarget => transform;
    private LocalPlayerRegistry _registry;
    private PlayerRuntimeRegistry _runtimeRegistry;
    private PlayerStatsNetwork _stats;

    [Networked] private Vector3 NetworkPosition { get; set; }
    [Networked] private Quaternion NetworkRotation { get; set; }

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

        if (HasStateAuthority)
        {
            EnforceGroundPlane();
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
        }
        else if (!Object.HasInputAuthority)
        {
            transform.SetPositionAndRotation(NetworkPosition, NetworkRotation);
        }

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
        if (ShouldSimulateMovement() && (_stats == null || _stats.IsDead == false))
        {
            if (GetInput<InputData>(out var input))
            {
                SimulateMovement(input, Runner.DeltaTime);
            }
        }

        if (!HasStateAuthority)
            return;

        EnforceGroundPlane();
        NetworkPosition = transform.position;
        NetworkRotation = transform.rotation;
    }

    public override void Render()
    {
        // Local input authority already uses predicted simulation in FixedUpdateNetwork.
        // Interpolating to authoritative snapshot here causes visible jitter.
        if (HasStateAuthority || Object.HasInputAuthority)
            return;

        float lerp = Mathf.Clamp01(_proxyLerp * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, NetworkPosition, lerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, NetworkRotation, lerp);
    }

    private bool ShouldSimulateMovement()
    {
        return HasStateAuthority || Object.HasInputAuthority;
    }

    private void SimulateMovement(in InputData input, float deltaTime)
    {
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

        EnforceGroundPlane();
    }

    private void EnforceGroundPlane()
    {
        var current = transform.position;
        if (Mathf.Approximately(current.y, 0f))
            return;

        current.y = 0f;
        transform.position = current;
    }
}

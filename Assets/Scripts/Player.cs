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

    [Inject]
    public void Construct(LocalPlayerRegistry registry)
    {
        _registry = registry;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
            _registry.SetLocalPlayer(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Object.HasInputAuthority)
            _registry.ClearLocalPlayer(this);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (GetInput<InputData>(out var input))
        {
            float deltaTime = Runner.DeltaTime;

            float turnAmount = input.Turn * _turnSpeed * deltaTime;
            transform.Rotate(0f, turnAmount, 0f);

            Vector3 move = transform.forward * (input.Forward * _moveSpeed * deltaTime);

            transform.position += move;
        }
    }
}
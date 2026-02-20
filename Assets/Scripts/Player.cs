using Fusion;
using UnityEngine;
using Zenject;

public class Player : NetworkBehaviour, ILocalPlayerCameraTarget
{
    public class Factory : PlaceholderFactory<Player> { }

    [SerializeField] private float _speed = 5f;

    public Transform FollowTarget => transform;
    private LocalPlayerRegistry _registry;

    public void Init(LocalPlayerRegistry registry)
    {
        _registry = registry;

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

        if (GetInput(out InputData input))
        {
            Vector3 dir = new Vector3(input.Move.x, 0, input.Move.y);
            transform.position += dir * (_speed * Runner.DeltaTime);
        }
    }
}
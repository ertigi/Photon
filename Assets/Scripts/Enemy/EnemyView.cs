using Fusion;
using Zenject;

public class EnemyView : NetworkBehaviour
{
    public class Factory : PlaceholderFactory<EnemyView> { }

    private EnemyRuntimeRegistry _runtimeRegistry;

    [Inject]
    public void Construct(EnemyRuntimeRegistry runtimeRegistry)
    {
        _runtimeRegistry = runtimeRegistry;
    }

    public override void Spawned()
    {
        _runtimeRegistry.Register(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _runtimeRegistry.Unregister(this);
    }

    private void OnDestroy()
    {
        _runtimeRegistry?.Unregister(this);
    }
}

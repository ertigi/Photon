using Fusion;
using UnityEngine;
using Zenject;

public class EnemyView : NetworkBehaviour
{
    public class Factory : PlaceholderFactory<EnemyView> { }

    [SerializeField] private float _proxyLerp = 20f;

    private EnemyRuntimeRegistry _runtimeRegistry;
    private EnemyController _ownerController;

    public EnemyController OwnerController => _ownerController;

    [Networked] private Vector3 NetworkPosition { get; set; }
    [Networked] private Quaternion NetworkRotation { get; set; }

    [Inject]
    public void Construct(EnemyRuntimeRegistry runtimeRegistry)
    {
        _runtimeRegistry = runtimeRegistry;
    }

    public override void Spawned()
    {
        _runtimeRegistry.Register(this);

        if (HasStateAuthority)
        {
            EnforceGroundPlane();
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
        }
        else
        {
            transform.SetPositionAndRotation(NetworkPosition, NetworkRotation);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        EnforceGroundPlane();
        NetworkPosition = transform.position;
        NetworkRotation = transform.rotation;
    }

    public override void Render()
    {
        if (HasStateAuthority)
            return;

        float lerp = Mathf.Clamp01(_proxyLerp * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, NetworkPosition, lerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, NetworkRotation, lerp);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _runtimeRegistry.Unregister(this);
        _ownerController = null;
    }

    private void OnDestroy()
    {
        _runtimeRegistry?.Unregister(this);
        _ownerController = null;
    }

    public void SetOwnerController(EnemyController ownerController)
    {
        _ownerController = ownerController;
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

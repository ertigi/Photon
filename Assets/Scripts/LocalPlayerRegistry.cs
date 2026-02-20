using UniRx;

public class LocalPlayerRegistry
{
    private readonly ReactiveProperty<ILocalPlayerCameraTarget> _local = new(null);

    public IReadOnlyReactiveProperty<ILocalPlayerCameraTarget> Local => _local;

    public void SetLocalPlayer(ILocalPlayerCameraTarget target) => _local.Value = target;

    public void ClearLocalPlayer(ILocalPlayerCameraTarget target)
    {
        if (_local.Value == target)
            _local.Value = null;
    }
}
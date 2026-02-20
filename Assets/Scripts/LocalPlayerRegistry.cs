using UniRx;

public class LocalPlayerRegistry
{
    public IReadOnlyReactiveProperty<ILocalPlayerCameraTarget> Local => _local;
    private readonly ReactiveProperty<ILocalPlayerCameraTarget> _local = new(null);

    public void SetLocalPlayer(ILocalPlayerCameraTarget target)
    {
        _local.Value = target;
    }

    public void ClearLocalPlayer(ILocalPlayerCameraTarget target)
    {
        if (_local.Value == target)
            _local.Value = null;
    }
}
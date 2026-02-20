using UniRx;
using UnityEngine;
using Zenject;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Vector3 _offset = new Vector3(0f, 15f, 0);
    [SerializeField] private float _followLerp = 10f;

    private Transform _target;
    private readonly CompositeDisposable _cd = new();

    [Inject]
    public void Construct(LocalPlayerRegistry registry)
    {
        registry.Local
            .Subscribe(t => _target = t?.FollowTarget)
            .AddTo(_cd);

        _target = registry.Local.Value.FollowTarget;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        Vector3 desiredPos = _target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, _followLerp * Time.deltaTime);
        transform.LookAt(_target.position);
    }

    private void OnDestroy() => _cd.Dispose();
}
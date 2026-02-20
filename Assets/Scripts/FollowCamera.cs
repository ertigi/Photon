using System;
using UniRx;
using UnityEngine;
using Zenject;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Vector3 _offset = new Vector3(0f, 15f, 0);
    [SerializeField] private float _followLerp = 10f;
    private readonly CompositeDisposable _cd = new();
    private Transform _target;

    [Inject]
    public void Construct(LocalPlayerRegistry registry)
    {
        registry.Local
            .Subscribe(t => SetTarget(t))
            .AddTo(_cd);
    }

    private void SetTarget(ILocalPlayerCameraTarget t)
    {
        _target = t?.FollowTarget;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        Vector3 desiredPos = _target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, _followLerp * Time.deltaTime);
    }

    private void OnDestroy()
    {
        _cd.Dispose();
    }
}

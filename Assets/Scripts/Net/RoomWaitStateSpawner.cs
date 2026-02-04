using System;
using Fusion;
using UnityEngine;
using Zenject;

public sealed class RoomWaitStateSpawner : IInitializable, IDisposable
{
    private readonly RunnerService _runnerService;
    private readonly SignalBus _bus;
    private readonly RoomWaitStateRegistry _registry;
    private readonly NetworkObject _prefab;

    public RoomWaitStateSpawner(
        RunnerService runnerService,
        SignalBus bus,
        RoomWaitStateRegistry registry,
        [Inject(Id = "RoomWaitStatePrefab")] NetworkObject prefab)
    {
        _runnerService = runnerService;
        _bus = bus;
        _registry = registry;
        _prefab = prefab;
    }

    public void Initialize()
    {
        _bus.Subscribe<PlayerJoinedSignal>(OnPlayerJoined);
        _bus.Subscribe<PlayerLeftSignal>(OnPlayerLeft);
        _bus.Subscribe<RunnerShutdownSignal>(OnShutdown);
    }

    public void Dispose()
    {
        _bus.Unsubscribe<PlayerJoinedSignal>(OnPlayerJoined);
        _bus.Unsubscribe<PlayerLeftSignal>(OnPlayerLeft);
        _bus.Unsubscribe<RunnerShutdownSignal>(OnShutdown);
    }

    private void OnPlayerJoined(PlayerJoinedSignal s)
    {
        if (!s.Runner.IsServer) 
            return;

        EnsureRoomWaitState(s.Runner);
        _registry.Current.ServerRegisterPlayer(s.Player);
    }

    private void OnPlayerLeft(PlayerLeftSignal s)
    {
        if (!s.Runner.IsServer) 
            return;
            
        if (_registry.Current == null) 
            return;

        _registry.Current.ServerUnregisterPlayer(s.Player);
    }

    private void OnShutdown(RunnerShutdownSignal s)
    {
        _registry.Set(null);
    }

    private void EnsureRoomWaitState(NetworkRunner runner)
    {
        if (_registry.Current != null)
            return;

        var obj = runner.Spawn(_prefab, Vector3.zero, Quaternion.identity, inputAuthority: null);
        var state = obj.GetComponent<RoomWaitState>();
        _registry.Set(state);
    }
}

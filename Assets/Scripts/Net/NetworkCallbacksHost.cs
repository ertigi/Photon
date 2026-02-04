using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Zenject;

public class NetworkCallbacksHost : IInitializable, IDisposable, INetworkRunnerCallbacks
{
    private RunnerService _runnerService;
    private SignalBus _signalBus;

    public NetworkCallbacksHost(RunnerService runnerService, SignalBus signalBus)
    {
        _runnerService = runnerService;
        _signalBus = signalBus;
    }

    public void Initialize()
    {
        _runnerService.Runner.AddCallbacks(this);
    }

    public void Dispose()
    {
        _runnerService.Runner.RemoveCallbacks(this);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        => _signalBus.Fire(new SessionListUpdatedSignal(sessionList));

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        => _signalBus.Fire(new PlayerJoinedSignal(runner, player));

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        => _signalBus.Fire(new PlayerLeftSignal(runner, player));

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        => _signalBus.Fire(new RunnerShutdownSignal(runner, shutdownReason));

    public void OnInput(NetworkRunner runner, NetworkInput input) {}
    
    public void OnConnectedToServer(NetworkRunner runner) {}
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
    public void OnSceneLoadDone(NetworkRunner runner) {}
    public void OnSceneLoadStart(NetworkRunner runner) {}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
}

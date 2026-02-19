using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;

public class FusionCallbacksHost : INetworkRunnerCallbacks, IDisposable
{
    private IInputProvider _inputProvider;
    private PlayerSpawner _playerSpawner;
    private NetworkRunner _networkRunner;

    public FusionCallbacksHost(IInputProvider inputProvider, PlayerSpawner playerSpawner, NetworkRunner networkRunner)
    {
        _inputProvider = inputProvider;
        _playerSpawner = playerSpawner;
        _networkRunner = networkRunner;
        _networkRunner.AddCallbacks(this);
    }

    public void Dispose()
    {
        _networkRunner.RemoveCallbacks(this);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new InputData
        {
            Move = _inputProvider.ReadMove()
        };
        input.Set(data);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        _playerSpawner.SpawnPlayer(runner, player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        _playerSpawner.DespawnPlayer(runner, player);
    }

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
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
}
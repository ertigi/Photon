using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FusionCallbacksHost : INetworkRunnerCallbacks, IDisposable
{
    private readonly IInputProvider _inputProvider;
    private readonly PlayerSpawner _playerSpawner;
    private readonly NetworkRunner _networkRunner;
    private readonly PlayerIdentityService _playerIdentityService;
    private readonly RoomAccessControlService _roomAccessControlService;
    private readonly ClientLobbyReturnService _clientLobbyReturnService;

    private readonly Dictionary<PlayerRef, PlayerStatsNetwork> _statsByPlayerRef = new();
    private readonly Dictionary<PlayerStatsNetwork, PlayerRef> _playerRefByStats = new();

    public FusionCallbacksHost(
        IInputProvider inputProvider,
        PlayerSpawner playerSpawner,
        NetworkRunner networkRunner,
        PlayerIdentityService playerIdentityService,
        RoomAccessControlService roomAccessControlService,
        ClientLobbyReturnService clientLobbyReturnService)
    {
        _inputProvider = inputProvider;
        _playerSpawner = playerSpawner;
        _networkRunner = networkRunner;
        _playerIdentityService = playerIdentityService;
        _roomAccessControlService = roomAccessControlService;
        _clientLobbyReturnService = clientLobbyReturnService;
        _networkRunner.AddCallbacks(this);
    }

    public void Dispose()
    {
        ClearDeathSubscriptions();
        _networkRunner.RemoveCallbacks(this);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var moveInput = _inputProvider.ReadMove();
        
        var data = new InputData
        {
            MoveX = moveInput.x,
            MoveY = moveInput.y
        };

        input.Set(data);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        var playerObject = _playerSpawner.SpawnPlayer(runner, player);
        RegisterPlayerToken(runner, player);
        SubscribePlayerDeath(player, playerObject);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        UnsubscribePlayerDeath(player);
        _roomAccessControlService.UnregisterConnectedPlayer(player);
        _playerSpawner.DespawnPlayer(runner, player);
    }

    public void OnConnectedToServer(NetworkRunner runner) {}
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        if (!runner.IsServer)
            return;

        string roomId = _roomAccessControlService.CurrentRoomId;
        if (string.IsNullOrWhiteSpace(roomId))
        {
            Debug.LogWarning("[Access] Connect refused: room id is not set on server.");
            request.Refuse();
            return;
        }

        if (!_playerIdentityService.TryDecodeToken(token, out var clientToken))
        {
            Debug.LogWarning("[Access] Connect refused: missing/invalid token.");
            request.Refuse();
            return;
        }

        if (!_roomAccessControlService.CanJoin(roomId, clientToken))
        {
            Debug.LogWarning($"[Access] Connect refused for banned token in room '{roomId}'.");
            request.Refuse();
            return;
        }

        request.Accept();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        if (runner.IsServer)
            return;

        _clientLobbyReturnService.RequestReturnToMenu();
    }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
    public void OnSceneLoadDone(NetworkRunner runner) {}
    public void OnSceneLoadStart(NetworkRunner runner) {}
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        ClearDeathSubscriptions();
        _roomAccessControlService.ClearConnectedPlayers();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}

    private void RegisterPlayerToken(NetworkRunner runner, PlayerRef player)
    {
        byte[] rawToken = runner.GetPlayerConnectionToken(player);

        if (_playerIdentityService.TryDecodeToken(rawToken, out var token))
        {
            _roomAccessControlService.RegisterConnectedPlayer(player, token);
            return;
        }

        if (player == runner.LocalPlayer)
            _roomAccessControlService.RegisterConnectedPlayer(player, _playerIdentityService.ClientToken);
    }

    private void SubscribePlayerDeath(PlayerRef player, NetworkObject playerObject)
    {
        if (playerObject == null)
            return;

        var stats = playerObject.GetComponent<PlayerStatsNetwork>();
        if (stats == null)
            return;

        UnsubscribePlayerDeath(player);

        stats.ServerDied += OnServerPlayerDied;
        _statsByPlayerRef[player] = stats;
        _playerRefByStats[stats] = player;
    }

    private void UnsubscribePlayerDeath(PlayerRef player)
    {
        if (!_statsByPlayerRef.TryGetValue(player, out var stats))
            return;

        if (stats != null)
        {
            stats.ServerDied -= OnServerPlayerDied;
            _playerRefByStats.Remove(stats);
        }

        _statsByPlayerRef.Remove(player);
    }

    private void ClearDeathSubscriptions()
    {
        foreach (var pair in _statsByPlayerRef)
        {
            if (pair.Value != null)
                pair.Value.ServerDied -= OnServerPlayerDied;
        }

        _statsByPlayerRef.Clear();
        _playerRefByStats.Clear();
    }

    private void OnServerPlayerDied(PlayerStatsNetwork stats)
    {
        if (!_networkRunner.IsServer || stats == null)
            return;

        if (!_playerRefByStats.TryGetValue(stats, out var player))
            return;

        _roomAccessControlService.MarkDeadByPlayer(player);
        _roomAccessControlService.UnregisterConnectedPlayer(player);
        UnsubscribePlayerDeath(player);

        if (!_networkRunner.IsRunning)
            return;

        if (player != _networkRunner.LocalPlayer)
            _networkRunner.Disconnect(player, null);
    }
}

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

public class LobbyService : IInitializable, IDisposable
{
    private RunnerService _runnerService;
    private SignalBus _signalBus;
    private bool _inLobby;
    
    public IReadOnlyList<SessionInfo> CurrentSessions => _currentSessions;
    private List<SessionInfo> _currentSessions = new();

    public LobbyService(RunnerService runnerService, SignalBus signalBus)
    {
        _runnerService = runnerService;
        _signalBus = signalBus;

        EnterLobbyAsync();
    }

    public void Initialize()
    {
        _signalBus.Subscribe<SessionListUpdatedSignal>(OnSessionListUpdated);
    }

    public void Dispose()
    {
        _signalBus.Subscribe<SessionListUpdatedSignal>(OnSessionListUpdated);
    }

    private void OnSessionListUpdated(SessionListUpdatedSignal s)
    {
        _currentSessions = new List<SessionInfo>(s.Sessions);
    }

    public async UniTask<bool> EnterLobbyAsync()
    {
        if (_inLobby) 
            return true;

        var result = await _runnerService.Runner.JoinSessionLobby(SessionLobby.Shared);
        
        if (!result.Ok)
        {
            Debug.LogError($"JoinSessionLobby failed: {result.ShutdownReason}");
            return false;
        }

        _inLobby = true;
        return true;
    }
}
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

public class LobbyService
{
    private RunnerService _runnerService;
    private bool _inLobby;

    public LobbyService(RunnerService runnerService)
    {
        _runnerService = runnerService;
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
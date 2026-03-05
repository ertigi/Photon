using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine.SceneManagement;
using Zenject;

public class ClientLobbyReturnService : IInitializable, IDisposable
{
    private const int MenuSceneIndex = 0;

    private readonly NetworkRunner _runner;
    private readonly LocalPlayerRegistry _localPlayerRegistry;

    private CancellationTokenSource _cts;
    private bool _isReturning;

    public ClientLobbyReturnService(NetworkRunner runner, LocalPlayerRegistry localPlayerRegistry)
    {
        _runner = runner;
        _localPlayerRegistry = localPlayerRegistry;
    }

    public void Initialize()
    {
        _cts = new CancellationTokenSource();
        WatchDeathLoopAsync(_cts.Token).Forget();
    }

    public void Dispose()
    {
        if (_cts == null)
            return;

        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    public void RequestReturnToMenu()
    {
        ReturnToMenuAsync(CancellationToken.None).Forget();
    }

    private async UniTaskVoid WatchDeathLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (ShouldReturnToMenu())
                {
                    await ReturnToMenuAsync(cancellationToken);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private bool ShouldReturnToMenu()
    {
        if (SceneManager.GetActiveScene().buildIndex == MenuSceneIndex)
            return false;

        if (!_runner.IsRunning)
            return true;

        return TryGetLocalPlayerStats(out var stats) && stats.IsDead;
    }

    private bool TryGetLocalPlayerStats(out PlayerStatsNetwork stats)
    {
        stats = null;

        var localTarget = _localPlayerRegistry.Local.Value as Player;
        if (localTarget == null)
            return false;

        if (localTarget.Object == null || !localTarget.Object.IsValid)
            return false;

        stats = localTarget.GetComponent<PlayerStatsNetwork>();
        if (stats == null || stats.Object == null || !stats.Object.IsValid)
            return false;

        return true;
    }

    private async UniTask ReturnToMenuAsync(CancellationToken cancellationToken)
    {
        if (_isReturning)
            return;

        _isReturning = true;

        try
        {
            if (_runner.IsRunning)
            {
                _runner.Shutdown(true, ShutdownReason.Ok, false);
                await UniTask.WaitUntil(() => !_runner.IsRunning, cancellationToken: cancellationToken);
            }

            if (SceneManager.GetActiveScene().buildIndex != MenuSceneIndex)
            {
                await SceneManager
                    .LoadSceneAsync(MenuSceneIndex)
                    .ToUniTask(cancellationToken: cancellationToken);
            }
        }
        finally
        {
            _isReturning = false;
        }
    }
}

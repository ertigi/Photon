using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UniRx;
using UnityEngine.SceneManagement;
using Zenject;

public class ClientLobbyReturnService : IInitializable, IDisposable, PlayerStatsNetwork.IRenderChangeListener
{
    private const int MenuSceneIndex = 0;

    private readonly NetworkRunner _runner;
    private readonly LocalPlayerRegistry _localPlayerRegistry;

    private readonly CompositeDisposable _subscriptions = new();
    private PlayerStatsNetwork _boundStats;
    private bool _isReturning;

    public ClientLobbyReturnService(NetworkRunner runner, LocalPlayerRegistry localPlayerRegistry)
    {
        _runner = runner;
        _localPlayerRegistry = localPlayerRegistry;
    }

    public void Initialize()
    {
        SubscribeLocalPlayerLifecycle();
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
        UnbindLocalStats();
    }

    public void RequestReturnToMenu()
    {
        ReturnToMenuAsync(CancellationToken.None).Forget();
    }

    public void NotifyDisconnected()
    {
        RequestReturnToMenu();
    }

    public void HandleHealthChanged(int hp, int maxHp, bool isDead)
    {
    }

    public void HandleProgressChanged(int xp, int level)
    {
    }

    public void HandleDeathChanged(bool isDead)
    {
        if (!isDead)
            return;

        RequestReturnToMenu();
    }

    private async UniTask ReturnToMenuAsync(CancellationToken cancellationToken)
    {
        if (_isReturning)
            return;

        _isReturning = true;

        try
        {
            UnbindLocalStats();

            if (_runner.IsRunning)
            {
                _ = _runner.Shutdown(true, ShutdownReason.Ok, false);
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

    private void SubscribeLocalPlayerLifecycle()
    {
        _subscriptions.Clear();

        _localPlayerRegistry.Local
            .DistinctUntilChanged()
            .Subscribe(OnLocalPlayerChanged)
            .AddTo(_subscriptions);
    }

    private void OnLocalPlayerChanged(ILocalPlayerCameraTarget localTarget)
    {
        if (!TryResolveLocalStats(localTarget, out var stats))
        {
            UnbindLocalStats();
            return;
        }

        BindLocalStats(stats);
    }

    private static bool TryResolveLocalStats(ILocalPlayerCameraTarget localTarget, out PlayerStatsNetwork stats)
    {
        stats = null;

        var localPlayer = localTarget as Player;
        if (localPlayer == null)
            return false;

        if (localPlayer.Object == null || !localPlayer.Object.IsValid)
            return false;

        stats = localPlayer.GetComponent<PlayerStatsNetwork>();
        if (stats == null || stats.Object == null || !stats.Object.IsValid)
            return false;

        return true;
    }

    private void BindLocalStats(PlayerStatsNetwork stats)
    {
        if (_boundStats == stats)
            return;

        UnbindLocalStats();
        _boundStats = stats;
        _boundStats.RegisterRenderListener(this, pushInitialState: true);
    }

    private void UnbindLocalStats()
    {
        if (_boundStats == null)
            return;

        _boundStats.UnregisterRenderListener(this);
        _boundStats = null;
    }
}

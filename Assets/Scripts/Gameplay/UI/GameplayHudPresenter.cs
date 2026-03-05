using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameplayHudPresenter : MonoBehaviour
{
    [SerializeField] private GameplayHudView _view;

    private LocalPlayerRegistry _localPlayerRegistry;
    private ExperienceCurveService _experienceCurveService;

    private PlayerStatsNetwork _localStats;
    private CancellationToken _destroyToken;

    [Inject]
    public void Construct(LocalPlayerRegistry localPlayerRegistry, ExperienceCurveService experienceCurveService)
    {
        _localPlayerRegistry = localPlayerRegistry;
        _experienceCurveService = experienceCurveService;
    }

    private void Awake()
    {
        _destroyToken = this.GetCancellationTokenOnDestroy();

        if (_view == null)
            _view = GetComponent<GameplayHudView>();
    }

    private void OnEnable()
    {
        RunAsync(_destroyToken).Forget();
    }

    private async UniTaskVoid RunAsync(CancellationToken cancellationToken)
    {
        if (_view == null)
            return;

        _view.SetVisible(false);

        while (!cancellationToken.IsCancellationRequested)
        {
            _localStats = await WaitForLocalPlayerStats(cancellationToken);
            if (cancellationToken.IsCancellationRequested || _localStats == null)
                return;

            _view.SetVisible(true);

            while (!cancellationToken.IsCancellationRequested && IsStatsStillLocal(_localStats))
            {
                UpdateHud(_localStats);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            _view.SetVisible(false);
            _localStats = null;
        }
    }

    private async UniTask<PlayerStatsNetwork> WaitForLocalPlayerStats(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (TryGetLocalStats(out var stats))
                return stats;

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        return null;
    }

    private bool TryGetLocalStats(out PlayerStatsNetwork stats)
    {
        stats = null;

        if (_localPlayerRegistry == null)
            return false;

        var localTarget = _localPlayerRegistry.Local.Value;
        var player = localTarget as Player;
        if (player == null)
            return false;

        if (player.Object == null || !player.Object.IsValid)
            return false;

        stats = player.GetComponent<PlayerStatsNetwork>();
        return stats != null && stats.Object != null && stats.Object.IsValid;
    }

    private bool IsStatsStillLocal(PlayerStatsNetwork stats)
    {
        if (stats == null || stats.Object == null || !stats.Object.IsValid)
            return false;

        if (!TryGetLocalStats(out var currentLocalStats))
            return false;

        return currentLocalStats == stats;
    }

    private void UpdateHud(PlayerStatsNetwork stats)
    {
        float hpNormalized = stats.MaxHP > 0
            ? (float)stats.HP / stats.MaxHP
            : 0f;

        int xpForNextLevel = Mathf.Max(1, _experienceCurveService.GetXpForNextLevel(stats.Level));
        float xpNormalized = (float)stats.XP / xpForNextLevel;

        _view.SetHpNormalized(hpNormalized);
        _view.SetXpNormalized(xpNormalized);
    }
}

using UniRx;
using UnityEngine;
using Zenject;

public class GameplayHudPresenter : MonoBehaviour, PlayerStatsNetwork.IRenderChangeListener
{
    [SerializeField] private GameplayHudView _view;

    private LocalPlayerRegistry _localPlayerRegistry;
    private ExperienceCurveService _experienceCurveService;

    private readonly CompositeDisposable _subscriptions = new();
    private PlayerStatsNetwork _boundStats;

    [Inject]
    public void Construct(LocalPlayerRegistry localPlayerRegistry, ExperienceCurveService experienceCurveService)
    {
        _localPlayerRegistry = localPlayerRegistry;
        _experienceCurveService = experienceCurveService;
    }

    private void Awake()
    {
        if (_view == null)
            _view = GetComponent<GameplayHudView>();

        if (_view != null)
            _view.SetVisible(false);
    }

    private void OnEnable()
    {
        SubscribeLocalPlayerLifecycle();
    }

    private void OnDisable()
    {
        _subscriptions.Clear();

        UnbindCurrentStats();
        if (_view != null)
            _view.SetVisible(false);
    }

    private void OnDestroy()
    {
        _subscriptions.Dispose();
    }

    private void SubscribeLocalPlayerLifecycle()
    {
        _subscriptions.Clear();

        if (_localPlayerRegistry == null)
            return;

        _localPlayerRegistry.Local
            .DistinctUntilChanged()
            .Subscribe(OnLocalPlayerChanged)
            .AddTo(_subscriptions);
    }

    private void OnLocalPlayerChanged(ILocalPlayerCameraTarget localTarget)
    {
        if (!TryResolveLocalStats(localTarget, out var stats))
        {
            UnbindCurrentStats();

            if (_view != null)
                _view.SetVisible(false);

            return;
        }

        BindToStats(stats);
    }

    private void BindToStats(PlayerStatsNetwork stats)
    {
        if (_boundStats == stats)
            return;

        UnbindCurrentStats();
        _boundStats = stats;
        _boundStats.RegisterRenderListener(this, pushInitialState: false);
        InitialHudUpdate(stats);
    }

    private bool TryResolveLocalStats(ILocalPlayerCameraTarget localTarget, out PlayerStatsNetwork stats)
    {
        stats = null;

        var player = localTarget as Player;
        if (player == null)
            return false;

        if (player.Object == null || !player.Object.IsValid)
            return false;

        stats = player.GetComponent<PlayerStatsNetwork>();
        return stats != null && stats.Object != null && stats.Object.IsValid;
    }

    private void UnbindCurrentStats()
    {
        if (_boundStats == null)
            return;

        _boundStats.UnregisterRenderListener(this);
        _boundStats = null;
    }

    private void InitialHudUpdate(PlayerStatsNetwork stats)
    {
        if (_view == null || stats == null || !stats.StateBufferIsValid)
            return;

        HandleHealthChanged(stats.HP, stats.MaxHP, stats.IsDead);
        HandleProgressChanged(stats.XP, stats.Level);
        HandleDeathChanged(stats.IsDead);
    }

    public void HandleHealthChanged(int hp, int maxHp, bool isDead)
    {
        if (_view == null)
            return;

        int safeMaxHp = Mathf.Max(1, maxHp);
        float hpNormalized = Mathf.Clamp01(hp / (float)safeMaxHp);
        _view.SetHpNormalized(hpNormalized);
    }

    public void HandleProgressChanged(int xp, int level)
    {
        if (_view == null)
            return;

        int xpForNextLevel = Mathf.Max(1, _experienceCurveService.GetXpForNextLevel(level));
        float xpNormalized = Mathf.Clamp01(xp / (float)xpForNextLevel);
        _view.SetXpNormalized(xpNormalized);
    }

    public void HandleDeathChanged(bool isDead)
    {
        if (_view == null)
            return;

        _view.SetVisible(!isDead);
    }
}

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


public sealed class RoomWaitingPresenter : MonoBehaviour
{
    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _startGameButton;

    private RunnerService _runnerService;
    private RoomWaitStateRegistry _registry;
    private MatchStartService _matchStart;

    private bool _isReady;

    [Inject]
    public void Construct(RunnerService runnerService, RoomWaitStateRegistry registry, MatchStartService matchStart)
    {
        _runnerService = runnerService;
        _registry = registry;
        _matchStart = matchStart;
    }

    private void Start()
    {
        _readyButton.onClick.AddListener(OnToggleReadyClicked);
        _startGameButton.onClick.AddListener(OnStartClicked);

        if(_runnerService.Runner.IsSceneAuthority)
            _startGameButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Минимальный polling для UI
        var state = _registry.Current;

        if (state == null)
            return;

        // TODO: обновить UI список игроков:
        // for i=0..PlayerCount: state.GetAt(i)
        // TODO: обновить кнопку Start: доступна ли
    }

    public void OnToggleReadyClicked()
    {
        var state = _registry.Current;
        if (state == null) 
            return;

        _isReady = !_isReady;
        state.SetLocalReady(_runnerService.Runner, _isReady);
    }

    public void OnStartClicked()
    {
        StartFlow().Forget();
    }

    private async UniTaskVoid StartFlow()
    {
        var state = _registry.Current;
        if (state == null) 
            return;

        await _matchStart.TryStartMatchAsync(state.AreAllReady);
    }
}
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

public sealed class RoomsBrowserPresenter : MonoBehaviour
{
    [SerializeField] private RoomInfoView _roomViewPrefab;
    [SerializeField] private RectTransform _parent;

    private List<SessionInfo> _sessions = new();
    private List<RoomInfoView> _views = new();

    private SignalBus _bus;
    private RoomService _roomService;
    private LobbyService _lobbyService;

    [Inject]
    public void Construct(SignalBus bus, RoomService roomService, LobbyService lobbyService)
    {
        _bus = bus;
        _roomService = roomService;
        _lobbyService = lobbyService;
    }

    private void OnEnable()
    {
        _bus.Subscribe<SessionListUpdatedSignal>(OnSessionListUpdated);
        
        if(_lobbyService.CurrentSessions.Count > 0)
        {
            _sessions = new (_lobbyService.CurrentSessions);
            CreateViews();
        }
    }

    private void OnDisable()
    {
        _bus.Unsubscribe<SessionListUpdatedSignal>(OnSessionListUpdated);
    }

    private void OnSessionListUpdated(SessionListUpdatedSignal s)
    {
        _sessions = new List<SessionInfo>(s.Sessions);
        CreateViews();
    }

    public void JoinByName(string sessionName)
    {
        JoinFlow(sessionName).Forget();
    }

    private async UniTaskVoid JoinFlow(string sessionName)
    {
        await _roomService.JoinRoomAsync(sessionName);
    }

    private void CreateViews()
    {
        if (_views.Count > 0)
        {
            _views.ForEach(o => Destroy(o.gameObject));
            _views.Clear();
        }

        if (_sessions.Count == 0)
            return;

        foreach (var item in _sessions)
        {
            var newPrefab = Instantiate(_roomViewPrefab, _parent);
            newPrefab.Init(this, item.Name);
            _views.Add(newPrefab);
        }
    }
}

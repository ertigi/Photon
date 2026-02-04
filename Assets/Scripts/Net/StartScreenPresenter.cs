using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public sealed class StartScreenPresenter : MonoBehaviour
{
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _browseRoomButton;

    [SerializeField] private Canvas _roomsBrowserCanvas;

    private LobbyService _lobby;
    private RoomService _rooms;


    [Inject]
    public void Construct(LobbyService lobby, RoomService rooms)
    {
        _lobby = lobby;
        _rooms = rooms;
    }

    private void Start()
    {
        _roomsBrowserCanvas.enabled = false;

        _createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        _browseRoomButton.onClick.AddListener(OnBrowseRoomsClicked);
    }

    public void OnCreateRoomClicked()
    {
        CreateRoomFlow().Forget();
    }

    public void OnBrowseRoomsClicked()
    {
        BrowseRoomsFlow().Forget();
    }

    private async UniTaskVoid CreateRoomFlow()
    {
        await _lobby.EnterLobbyAsync();
        await _rooms.CreateRoomAsync($"Room_{Random.Range(1000, 9999)}");
    }

    private async UniTaskVoid BrowseRoomsFlow()
    {
        if(await _lobby.EnterLobbyAsync())
        {
            _roomsBrowserCanvas.enabled = true;
        }
    }
}
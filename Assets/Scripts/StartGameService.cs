using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

public class StartGameService
{
    private readonly NetworkRunner _runner;
    private readonly NetworkSceneManagerDefault _sceneManager;
    private readonly INetworkObjectProvider _networkObjectProvider;
    private readonly PlayerIdentityService _playerIdentityService;
    private readonly RoomAccessControlService _roomAccessControlService;

    public NetworkRunner Runner => _runner;

    public StartGameService(
        NetworkRunner runner,
        NetworkSceneManagerDefault sceneManager,
        INetworkObjectProvider networkObjectProvider,
        PlayerIdentityService playerIdentityService,
        RoomAccessControlService roomAccessControlService)
    {
        _runner = runner;
        _sceneManager = sceneManager;
        _networkObjectProvider = networkObjectProvider;
        _playerIdentityService = playerIdentityService;
        _roomAccessControlService = roomAccessControlService;

        Application.runInBackground = true;
    }

    public async UniTask StartAsHost(string roomId)
    {
        _runner.ProvideInput = true;
        _roomAccessControlService.BeginRoomSession(roomId);

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(1));

        var args = new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = roomId,
            Scene = sceneInfo,
            SceneManager = _sceneManager,
            ObjectProvider = _networkObjectProvider,
            ConnectionToken = _playerIdentityService.GetConnectionToken()
        };

        var result = await _runner.StartGame(args);

        if (!result.Ok)
        {
            Debug.LogError($"[Fusion] Host failed: {result.ShutdownReason}");
        }
    }

    public async UniTask StartAsClient(string roomId)
    {
        _runner.ProvideInput = true;
        _roomAccessControlService.SetCurrentRoom(roomId);

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(1));

        var args = new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = roomId,
            Scene = sceneInfo,
            SceneManager = _sceneManager,
            ObjectProvider = _networkObjectProvider,
            ConnectionToken = _playerIdentityService.GetConnectionToken()
        };

        var result = await _runner.StartGame(args);

        if (!result.Ok)
        {
            Debug.LogError($"[Fusion] Client failed: {result.ShutdownReason}");
        }
    }
}

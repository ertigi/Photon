using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UniRx;

public class StartGameService
{
    private readonly NetworkRunner _runner;
    private readonly NetworkSceneManagerDefault _sceneManager;

    public NetworkRunner Runner => _runner;

    public StartGameService(NetworkRunner runner, NetworkSceneManagerDefault sceneManager)
    {
        _runner = runner;
        _sceneManager = sceneManager;
    }

    public async UniTask StartAsHost(string roomId)
    {
        _runner.ProvideInput = true;

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(1));

        var args = new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = roomId,
            Scene = sceneInfo,
            SceneManager = _sceneManager
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

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(1));

        var args = new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = roomId,
            Scene = sceneInfo,
            SceneManager = _sceneManager
        };

        var result = await _runner.StartGame(args);

        if (!result.Ok)
        {
            Debug.LogError($"[Fusion] Client failed: {result.ShutdownReason}");
        }
    }
}

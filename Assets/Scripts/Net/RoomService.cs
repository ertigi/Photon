using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public sealed class RoomService
{
    private RunnerService _runnerService;
    private SignalBus _signalBus;
    private int _roomWaitingSceneIndex;

    public RoomService(RunnerService runnerService, SignalBus signalBus, int roomWaitingSceneIndex)
    {
        _runnerService = runnerService;
        _signalBus = signalBus;
        _roomWaitingSceneIndex = roomWaitingSceneIndex;
    }

    public async UniTask<bool> CreateRoomAsync(string sessionName)
    {
        var result = await _runnerService.Runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            SceneManager = _runnerService.SceneManager,
        });

        if (!result.Ok)
        {
            Debug.LogError($"CreateRoom StartGame failed: {result.ShutdownReason}");
            return false;
        }

        _signalBus.Fire(new JoinedSessionSignal(sessionName));

        // В Host хост = SceneAuthority, может грузить ожидание
        _runnerService.Runner.LoadScene(SceneRef.FromIndex(_roomWaitingSceneIndex), LoadSceneMode.Single);
        return true;
    }

    public async UniTask<bool> JoinRoomAsync(string sessionName)
    {
        var result = await _runnerService.Runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = _runnerService.SceneManager,
        });

        if (!result.Ok)
        {
            Debug.LogError($"JoinRoom StartGame failed: {result.ShutdownReason}");
            return false;
        }

        _signalBus.Fire(new JoinedSessionSignal(sessionName));
        // Клиент сцену сам не грузит — он получит синхронную загрузку от хоста
        return true;
    }

    public async UniTask LeaveRoomAsync()
    {
        await _runnerService.Runner.Shutdown();
        _signalBus.Fire(new LeftSessionSignal());
    }
}
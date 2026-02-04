using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine.SceneManagement;

public sealed class MatchStartService
{
    private readonly RunnerService _runnerService;
    private readonly int _gameplaySceneIndex;

    public MatchStartService(RunnerService runnerService, int gameplaySceneIndex)
    {
        _runnerService = runnerService;
        _gameplaySceneIndex = gameplaySceneIndex;
    }

    public UniTask<bool> TryStartMatchAsync(bool allReady)
    {
        if (!_runnerService.Runner.IsSceneAuthority) 
            return UniTask.FromResult(false);

        if (!allReady) 
            return UniTask.FromResult(false);

        _runnerService.Runner.LoadScene(SceneRef.FromIndex(_gameplaySceneIndex), LoadSceneMode.Single);
        return UniTask.FromResult(true);
    }
}
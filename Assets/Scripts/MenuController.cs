using Cysharp.Threading.Tasks;

public class MenuController
{
    private StartGameService _startGameService;

    public MenuController(StartGameService startGameService)
    {
        _startGameService = startGameService;
    }

    public UniTask CreateRoom(string roomId)
    {
        return _startGameService.StartAsHost(roomId);
    }

    public UniTask JoinRoom(string roomId)
    {
        return _startGameService.StartAsClient(roomId);
    }
}

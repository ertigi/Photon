using Cysharp.Threading.Tasks;
using UniRx;
using System;

public class MenuViewModel
{
    public ReactiveProperty<string> RoomId { get; } = new("default_room");
    public ReadOnlyReactiveProperty<bool> CanSubmit { get; }
    public ReactiveProperty<string> Message { get; } = new(string.Empty);

    private ReactiveProperty<bool> _isBusy = new(false);
    private CompositeDisposable _disposables = new();
    private MenuController _menuController;


    public MenuViewModel(MenuController menuController)
    {
        _menuController = menuController;

        CanSubmit = RoomId
            .Select(id => !string.IsNullOrWhiteSpace(id))
            .CombineLatest(_isBusy, (hasId, busy) => hasId && !busy)
            .ToReadOnlyReactiveProperty()
            .AddTo(_disposables);
    }

    public void CreateRoom()
    {
        Run(async () => { await _menuController.CreateRoom(RoomId.Value);}).Forget();
    }

    public void JoinRoom()
    {
        Run(async () => { await _menuController.JoinRoom(RoomId.Value);}).Forget();
    }

    private async UniTaskVoid Run(Func<UniTask> action)
    {
        if (_isBusy.Value) 
            return;

        _isBusy.Value = true;
        Message.Value = "Connecting...";

        try
        {
            await action();
        }
        catch (Exception e)
        {
            Message.Value = e.Message;
        }
        finally
        {
            _isBusy.Value = false;
        }
    }
}

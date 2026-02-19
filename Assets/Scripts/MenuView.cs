using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using Zenject;
using TMPro;
using UnityEngine.UI;

public sealed class MenuView : MonoBehaviour
{
    [SerializeField] private TMP_InputField _roomIdInput;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private TMP_Text _statusText;

    private CompositeDisposable _compositeDisposable = new();
    private MenuViewModel _menuVM;

    [Inject]
    public void Construct(MenuViewModel vm)
    {
        _menuVM = vm;
    }

    private void Awake()
    {
        Bind();
    }

    private void Bind()
    {
        _roomIdInput.onValueChanged.AsObservable()
            .Subscribe(text => _menuVM.RoomId.Value = text)
            .AddTo(_compositeDisposable);

        _menuVM.CanSubmit
            .Subscribe(can =>
            {
                _createButton.interactable = can;
                _joinButton.interactable = can;
                _roomIdInput.interactable = can;
            })
            .AddTo(_compositeDisposable);

        _menuVM.Message
            .Where(err => _statusText != null)
            .Subscribe(err =>
            {
                _statusText.text = err;
            })
            .AddTo(_compositeDisposable);

        _createButton.onClick.AddListener(_menuVM.CreateRoom);
        _joinButton.onClick.AddListener(_menuVM.JoinRoom);
    }

    private void OnDestroy()
    {
        _compositeDisposable.Dispose();
    }
}
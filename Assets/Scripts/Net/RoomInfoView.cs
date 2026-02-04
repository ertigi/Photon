using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoView : MonoBehaviour
{
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private Button _joinButton;

    private string _roomName;
    private RoomsBrowserPresenter _roomsBrowserPresenter;

    public void Init(RoomsBrowserPresenter roomsBrowserPresenter, string roomName)
    {
        _roomsBrowserPresenter = roomsBrowserPresenter;
        _roomName = roomName;
        _roomNameText.text = _roomName;

        _joinButton.onClick.AddListener(OnJoinClick);
    }
    
    private void OnJoinClick()
    {
        if(_roomName == null)
            return;

        _roomsBrowserPresenter.JoinByName(_roomName);
    }
}
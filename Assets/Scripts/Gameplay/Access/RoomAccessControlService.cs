using System.Collections.Generic;
using Fusion;

public class RoomAccessControlService
{
    private readonly Dictionary<string, HashSet<string>> _bannedTokensByRoom = new();
    private readonly Dictionary<PlayerRef, string> _connectedPlayerTokens = new();

    private string _currentRoomId = string.Empty;

    public string CurrentRoomId => _currentRoomId;

    public void BeginRoomSession(string roomId)
    {
        _currentRoomId = Normalize(roomId);

        if (string.IsNullOrEmpty(_currentRoomId))
            return;

        _bannedTokensByRoom[_currentRoomId] = new HashSet<string>();
        _connectedPlayerTokens.Clear();
    }

    public void SetCurrentRoom(string roomId)
    {
        _currentRoomId = Normalize(roomId);
    }

    public bool CanJoin(string roomId, string token)
    {
        roomId = Normalize(roomId);
        token = Normalize(token);

        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(token))
            return false;

        if (!_bannedTokensByRoom.TryGetValue(roomId, out var roomBans))
            return true;

        return !roomBans.Contains(token);
    }

    public void RegisterConnectedPlayer(PlayerRef player, string token)
    {
        token = Normalize(token);

        if (string.IsNullOrEmpty(token))
            return;

        _connectedPlayerTokens[player] = token;
    }

    public void UnregisterConnectedPlayer(PlayerRef player)
    {
        _connectedPlayerTokens.Remove(player);
    }

    public bool TryGetConnectedToken(PlayerRef player, out string token)
    {
        return _connectedPlayerTokens.TryGetValue(player, out token);
    }

    public bool MarkDeadByPlayer(PlayerRef player)
    {
        if (string.IsNullOrEmpty(_currentRoomId))
            return false;

        if (!_connectedPlayerTokens.TryGetValue(player, out var token))
            return false;

        return MarkTokenBanned(_currentRoomId, token);
    }

    public bool MarkTokenBanned(string roomId, string token)
    {
        roomId = Normalize(roomId);
        token = Normalize(token);

        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(token))
            return false;

        if (!_bannedTokensByRoom.TryGetValue(roomId, out var roomBans))
        {
            roomBans = new HashSet<string>();
            _bannedTokensByRoom[roomId] = roomBans;
        }

        return roomBans.Add(token);
    }

    public void ClearConnectedPlayers()
    {
        _connectedPlayerTokens.Clear();
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim();
    }
}

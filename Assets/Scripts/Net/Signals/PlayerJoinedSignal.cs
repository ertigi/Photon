using Fusion;

public readonly struct PlayerJoinedSignal
{
    public readonly NetworkRunner Runner;
    public readonly PlayerRef Player;
    public PlayerJoinedSignal(NetworkRunner r, PlayerRef p) { Runner = r; Player = p; }
}

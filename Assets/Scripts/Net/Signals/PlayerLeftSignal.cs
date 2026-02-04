using Fusion;

public readonly struct PlayerLeftSignal
{
    public readonly NetworkRunner Runner;
    public readonly PlayerRef Player;
    public PlayerLeftSignal(NetworkRunner r, PlayerRef p) { Runner = r; Player = p; }
}

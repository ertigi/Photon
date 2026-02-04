using Fusion;

public sealed class RoomWaitState : NetworkBehaviour
{
    public const int MaxPlayers = 8;

    [Networked] public int PlayerCount { get; private set; }
    [Networked] private NetworkArray<PlayerRef> Players => default;
    [Networked] private NetworkArray<NetworkBool> Ready => default;

    public bool AreAllReady
    {
        get
        {
            if (PlayerCount <= 0)
                return false;

            for (int i = 0; i < PlayerCount; i++)
            {
                if (!Ready[i])
                    return false;
            }
            return true;
        }
    }

    public void ServerRegisterPlayer(PlayerRef player)
    {
        if (!Object.HasStateAuthority)
            return;

        for (int i = 0; i < PlayerCount; i++)
            if (Players[i] == player)
                return;

        if (PlayerCount >= MaxPlayers)
            return;

        Players.Set(PlayerCount, player);
        Ready.Set(PlayerCount, false);
        PlayerCount++;
    }

    public void ServerUnregisterPlayer(PlayerRef player)
    {
        if (!Object.HasStateAuthority)
            return;

        int idx = -1;
        for (int i = 0; i < PlayerCount; i++)
        {
            if (Players[i] == player)
            {
                idx = i;
                break;
            }
        }

        if (idx < 0) 
            return;

        int last = PlayerCount - 1;
        Players.Set(idx, Players[last]);
        Ready.Set(idx, Ready[last]);

        Players.Set(last, default);
        Ready.Set(last, false);

        PlayerCount--;
    }

    public void SetLocalReady(NetworkRunner runner, bool isReady)
    {
        if (Object.HasStateAuthority)
            ServerSetReady(runner.LocalPlayer, isReady);
        else
            RPC_SetReady(runner.LocalPlayer, isReady);
    }

    private void ServerSetReady(PlayerRef player, bool isReady)
    {
        if (!Object.HasStateAuthority) 
            return;

        for (int i = 0; i < PlayerCount; i++)
        {
            if (Players[i] == player)
            {
                Ready.Set(i, isReady);
                return;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetReady(PlayerRef player, bool isReady) => ServerSetReady(player, isReady);

    public (PlayerRef player, bool ready) GetAt(int index) => (Players[index], Ready[index]);
}
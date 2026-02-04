public sealed class RoomWaitStateRegistry
{
    public RoomWaitState Current { get; private set; }

    public void Set(RoomWaitState state) 
        => Current = state;

    public void Clear(RoomWaitState state)
    {
        if (Current == state) 
            Current = null;
    }
}
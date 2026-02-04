using Fusion;

public readonly struct RunnerShutdownSignal
{
    public readonly NetworkRunner Runner;
    public readonly ShutdownReason Reason;
    public RunnerShutdownSignal(NetworkRunner r, ShutdownReason reason) { Runner = r; Reason = reason; }
}
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public readonly struct SessionListUpdatedSignal
{
    public readonly IReadOnlyList<SessionInfo> Sessions;
    public SessionListUpdatedSignal(IReadOnlyList<SessionInfo> sessions) => Sessions = sessions;
}

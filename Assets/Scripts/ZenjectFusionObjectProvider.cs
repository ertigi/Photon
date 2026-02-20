using System;
using System.Collections.Generic;
using Fusion;

public sealed class ZenjectFusionObjectProvider : INetworkObjectProvider
{
    private readonly Dictionary<NetworkObjectGuid, Func<NetworkObject>> _createByGuid;

    public ZenjectFusionObjectProvider(Dictionary<NetworkObjectGuid, Func<NetworkObject>> createByGuid)
    {
        _createByGuid = createByGuid;
    }
    
    public NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
    {
        var prefabGuid = runner.Prefabs.GetGuid(context.PrefabId);

        if (_createByGuid.TryGetValue(prefabGuid, out var factory))
        {
            result = factory.Invoke();
            if (context.DontDestroyOnLoad)
                UnityEngine.Object.DontDestroyOnLoad(result.gameObject);

            return NetworkObjectAcquireResult.Success;
        }

        result = null;
        return NetworkObjectAcquireResult.Failed;
    }

    public NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid)
    {
        return runner.Prefabs.GetId(prefabGuid);
    }

    public void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
    {
        if (context.Object != null)
            UnityEngine.Object.Destroy(context.Object.gameObject);
    }
}
using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private NetworkRunner _runner;
    [SerializeField] private NetworkSceneManagerDefault _sceneManager;
    [SerializeField] private PrefabsConfig _prefabsConfig;


    public override void InstallBindings()
    {
        Container.BindInstance(_runner).AsSingle();
        Container.BindInstance(_sceneManager).AsSingle();
        Container.BindInstance(_prefabsConfig).AsSingle();

        Container.Bind<IInputProvider>().To<KeyboardInputProvider>().AsSingle();

        Container.Bind<LocalPlayerRegistry>().AsSingle();
        Container.BindFactory<Player, Player.Factory>().FromComponentInNewPrefab(_prefabsConfig.PlayerPrefabSource);
        Container.Bind<INetworkObjectProvider>().FromMethod(ctx => CreateObjectProvider(ctx.Container)).AsSingle();

        Container.Bind<PlayerSpawner>().AsSingle();
        Container.BindInterfacesTo<FusionCallbacksHost>().AsSingle();

        Container.Bind<StartGameService>().AsSingle();
    }

    private INetworkObjectProvider CreateObjectProvider(DiContainer c)
    {
        var playerFactory = c.Resolve<Player.Factory>();

        NetworkObjectGuid playerGuid = (NetworkObjectGuid)_prefabsConfig.NetworkPlayerPrefab;

        var map = new Dictionary<NetworkObjectGuid, Func<NetworkObject>>
        {
            [playerGuid] = () => playerFactory.Create().GetComponent<NetworkObject>()
        };

        return new ZenjectFusionObjectProvider(map);
    }
}

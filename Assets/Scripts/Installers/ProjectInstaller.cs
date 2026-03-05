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
    [SerializeField] private PlayerStatsConfig _playerStatsConfig;
    [SerializeField] private ProgressionConfig _progressionConfig;
    [SerializeField] private LootConfig _lootConfig;


    public override void InstallBindings()
    {
        Container.BindInstance(_runner).AsSingle();
        Container.BindInstance(_sceneManager).AsSingle();
        Container.BindInstance(_prefabsConfig).AsSingle();
        Container.BindInstance(_playerStatsConfig).AsSingle();
        Container.BindInstance(_progressionConfig).AsSingle();
        Container.BindInstance(_lootConfig).AsSingle();

        Container.Bind<IInputProvider>().To<KeyboardInputProvider>().AsSingle();

        Container.Bind<LocalPlayerRegistry>().AsSingle();
        Container.Bind<PlayerRuntimeRegistry>().AsSingle();
        Container.Bind<EnemyRuntimeRegistry>().AsSingle();
        Container.Bind<EnemyTargetingService>().AsSingle();
        Container.Bind<PlayerDamageService>().AsSingle();
        Container.Bind<EnemyDamageService>().AsSingle();
        Container.Bind<LootApplyService>().AsSingle();
        Container.Bind<EnemyLootDropService>().AsSingle();
        Container.Bind<ExperienceCurveService>().AsSingle();
        Container.Bind<LevelBonusRollService>().AsSingle();

        Container.BindFactory<Player, Player.Factory>().FromComponentInNewPrefab(_prefabsConfig.PlayerPrefabSource);
        Container.BindFactory<EnemyView, EnemyView.Factory>().FromComponentInNewPrefab(_prefabsConfig.EnemyPrefabSource);
        Container.BindFactory<LootPickupNetwork, LootPickupNetwork.PotionFactory>().FromComponentInNewPrefab(_prefabsConfig.PotionLootPrefabSource);
        Container.BindFactory<LootPickupNetwork, LootPickupNetwork.XpCrystalFactory>().FromComponentInNewPrefab(_prefabsConfig.XpCrystalLootPrefabSource);

        Container.Bind<INetworkObjectProvider>().FromMethod(ctx => CreateObjectProvider(ctx.Container)).AsSingle();

        Container.Bind<PlayerSpawner>().AsSingle();
        Container.BindInterfacesTo<FusionCallbacksHost>().AsSingle().NonLazy();

        Container.Bind<StartGameService>().AsSingle();
    }

    private INetworkObjectProvider CreateObjectProvider(DiContainer c)
    {
        var playerFactory = c.Resolve<Player.Factory>();
        var enemyFactory = c.Resolve<EnemyView.Factory>();
        var potionLootFactory = c.Resolve<LootPickupNetwork.PotionFactory>();
        var xpCrystalLootFactory = c.Resolve<LootPickupNetwork.XpCrystalFactory>();

        NetworkObjectGuid playerGuid = (NetworkObjectGuid)_prefabsConfig.NetworkPlayerPrefab;
        NetworkObjectGuid enemyGuid = (NetworkObjectGuid)_prefabsConfig.NetworkEnemyPrefab;
        NetworkObjectGuid potionLootGuid = (NetworkObjectGuid)_prefabsConfig.NetworkPotionLootPrefab;
        NetworkObjectGuid xpCrystalLootGuid = (NetworkObjectGuid)_prefabsConfig.NetworkXpCrystalLootPrefab;

        var map = new Dictionary<NetworkObjectGuid, Func<NetworkObject>>
        {
            [playerGuid] = () => playerFactory.Create().GetComponent<NetworkObject>(),
            [enemyGuid] = () => enemyFactory.Create().GetComponent<NetworkObject>(),
            [potionLootGuid] = () => potionLootFactory.Create().GetComponent<NetworkObject>(),
            [xpCrystalLootGuid] = () => xpCrystalLootFactory.Create().GetComponent<NetworkObject>()
        };

        return new ZenjectFusionObjectProvider(map);
    }
}

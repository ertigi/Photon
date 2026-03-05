using Zenject;

public class GameplayInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<PlayerRuntimeRegistry>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<EnemyRuntimeRegistry>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<EnemySpawnService>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<EnemyTargetingService>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<PlayerDamageService>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<EnemyDamageService>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<LootApplyService>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<EnemyLootDropService>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<ExperienceCurveService>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<LevelBonusRollService>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<PlayerStatsConfig>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<ProgressionConfig>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<LootConfig>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<EnemySpawnConfig>()
            .FromResolve(null, InjectSources.Parent)
            .AsSingle();

        Container.Bind<GameplayHudView>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<GameplayHudPresenter>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}

using Zenject;

public class GameplayInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<GameplayHudView>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<GameplayHudPresenter>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}

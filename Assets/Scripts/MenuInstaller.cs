using Zenject;

public class MenuInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<MenuViewModel>().AsSingle();
        Container.Bind<MenuController>().AsSingle();
        Container.Bind<StartGameService>().AsSingle();
    }
}
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
 
        Container.BindInterfacesAndSelfTo<KeyboardInputProvider>().AsSingle();
        Container.Bind<PlayerSpawner>().AsSingle();
        Container.BindInterfacesTo<FusionCallbacksHost>().AsSingle();
        Container.Bind<StartGameService>().AsSingle();
        Container.Bind<LocalPlayerRegistry>().AsSingle();
    }
}

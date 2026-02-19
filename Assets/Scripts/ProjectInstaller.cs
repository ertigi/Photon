using Fusion;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private NetworkRunner _runner;
    [SerializeField] private NetworkSceneManagerDefault _sceneManager;

    [SerializeField] private NetworkPrefabRef _playerPrefab;

    public override void InstallBindings()
    {
        Container.BindInstance(_runner).AsSingle();
        Container.BindInstance(_sceneManager).AsSingle();
    }
}

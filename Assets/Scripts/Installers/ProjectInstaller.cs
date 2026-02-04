using Fusion;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [Header("Fusion")]
    [SerializeField] private NetworkRunner _runner;

    [Header("Room Waiting")]
    [SerializeField] private NetworkObject _roomWaitStatePrefab;

    [Header("Scenes (Build Settings indices)")]
    [SerializeField] private int _startScreenSceneIndex = 0;
    [SerializeField] private int _roomWaitingSceneIndex = 1;
    [SerializeField] private int _gameplaySceneIndex = 2;

    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<SessionListUpdatedSignal>();
        Container.DeclareSignal<JoinedSessionSignal>();
        Container.DeclareSignal<LeftSessionSignal>();
        Container.DeclareSignal<PlayerJoinedSignal>();
        Container.DeclareSignal<PlayerLeftSignal>();
        Container.DeclareSignal<RunnerShutdownSignal>();

        Container.Bind<RunnerService>()
            .AsSingle()
            .WithArguments(_runner);

        Container.BindInterfacesAndSelfTo<NetworkCallbacksHost>()
            .AsSingle();

        Container.Bind<LobbyService>().AsSingle().NonLazy();
        Container.Bind<RoomService>()
            .AsSingle()
            .WithArguments(_roomWaitingSceneIndex).NonLazy();;

        Container.Bind<MatchStartService>()
            .AsSingle()
            .WithArguments(_gameplaySceneIndex).NonLazy();;

        Container.Bind<NetworkObject>()
            .WithId("RoomWaitStatePrefab")
            .FromInstance(_roomWaitStatePrefab)
            .AsSingle().NonLazy();;

        Container.Bind<RoomWaitStateRegistry>().AsSingle().NonLazy();;
        Container.BindInterfacesTo<RoomWaitStateSpawner>().AsSingle().NonLazy();;
    }
}

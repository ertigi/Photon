using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class RunnerService
{
    public NetworkRunner Runner { get; private set; }
    public NetworkSceneManagerDefault SceneManager { get; private set; }


    public RunnerService(NetworkRunner runner)
    {
        Runner = runner;
    }
}
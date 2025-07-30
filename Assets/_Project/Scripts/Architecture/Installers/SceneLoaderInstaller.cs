using _Project.Scripts.Architecture.Core;
using _Project.Scripts.Architecture.ScenarioDataTransfer;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Architecture.Installers
{
    public class SceneLoaderInstaller : MonoInstaller
    {
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private ScenarioData _scenarioData;

        public override void InstallBindings()
        {
            if (_sceneLoader != null)
            {
                Container.Bind<SceneLoader>()
                    .FromInstance(_sceneLoader)
                    .AsSingle();
            }
            else
            {
                Debug.LogError("SceneLoader is not assigned in inspector!");
            }

            if (_scenarioData != null)
            {
                Container.Bind<ScenarioData>()
                    .FromInstance(_scenarioData)
                    .AsSingle();
            }
            else
            {
                Debug.LogError("ScenarioData is not assigned in inspector!");
            }
        }
    }
}
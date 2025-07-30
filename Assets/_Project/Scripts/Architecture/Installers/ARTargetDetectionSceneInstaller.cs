using _Project.Scripts.Architecture.UI.Components;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Architecture.Installers
{
    public class ARTargetDetectionSceneInstaller : MonoInstaller
    {
        [SerializeField] private TargetDataList _targetDataList;

        public override void InstallBindings()
        {
            if (_targetDataList != null)
            {
                Container.Bind<TargetDataList>()
                    .FromInstance(_targetDataList)
                    .AsSingle();
            }
            else
            {
                Debug.LogError("TargetDataList is not assigned in inspector!");
            }

            // Bind UI components
            Container.Bind<UntargetUI>()
                .FromComponentInHierarchy()
                .AsSingle();
        }
    }
}
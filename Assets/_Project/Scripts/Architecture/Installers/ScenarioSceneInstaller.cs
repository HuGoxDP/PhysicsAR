using _Project.Scripts.Architecture.UI.Components;
using _Project.Scripts.Architecture.UI.Core;
using _Project.Scripts.Architecture.UI.Interfaces;
using Zenject;

namespace _Project.Scripts.Architecture.Installers
{
    public class ScenarioSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Then bind UI infrastructure
            Container.Bind<UIStateManager>().AsSingle();

            Container.Bind<IUIFactory>()
                .To<UIFactory>()
                .AsSingle();

            Container.Bind<IUIManager>()
                .To<UIManager>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container.Bind<InteractiveScenarioUI>()
                .FromComponentInHierarchy()
                .AsSingle();

            Container.Bind<AnimationScenarioUI>()
                .FromComponentInHierarchy()
                .AsSingle();

            // Finally, bind the initializer
            Container.BindInterfacesTo<UIRegistrationInitializer>()
                .AsSingle();
            Container.BindInterfacesTo<ScenarioInitializer>()
                .AsSingle();
        }
    }
}
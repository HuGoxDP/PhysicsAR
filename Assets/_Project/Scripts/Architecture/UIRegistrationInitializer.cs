using _Project.Scripts.Architecture.UI.Components;
using _Project.Scripts.Architecture.UI.Core;
using _Project.Scripts.Architecture.UI.Interfaces;
using Zenject;

namespace _Project.Scripts.Architecture
{
    public class UIRegistrationInitializer : IInitializable
    {
        private readonly AnimationScenarioUI _animationUI;

        private readonly InteractiveScenarioUI _interactiveUI;

        // The rest of your class remains the same
        private readonly IUIFactory _uiFactory;

        public UIRegistrationInitializer(
            IUIFactory uiFactory,
            InteractiveScenarioUI interactiveUI,
            AnimationScenarioUI animationUI)
        {
            _uiFactory = uiFactory;
            _interactiveUI = interactiveUI;
            _animationUI = animationUI;
        }

        public void Initialize()
        {
            _uiFactory.RegisterUI(UIType.InteractiveScenario, _interactiveUI);
            _uiFactory.RegisterUI(UIType.AnimationScenario, _animationUI);
        }
    }
}
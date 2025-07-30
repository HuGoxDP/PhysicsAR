using _Project.Scripts.Architecture.ScenarioDataTransfer;
using _Project.Scripts.Architecture.UI.Components;
using _Project.Scripts.Architecture.UI.Interfaces;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Architecture.UI.Core
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [Inject] private ScenarioData _scenarioData;
        [Inject] private UIStateManager _stateManager;
        [Inject] private IUIFactory _uiFactory;

        private void Awake()
        {
            if (_uiFactory == null)
            {
                Debug.LogError("UI dependencies not properly injected");
            }
        }

        private void Start()
        {
            if (_scenarioData != null)
            {
            }
        }

        public BaseUI CurrentUI => _stateManager.CurrentUI;

        /// <summary>
        /// Switches to the specified UI type
        /// </summary>
        public void SwitchToUI(UIType uiType)
        {
            var ui = _uiFactory.GetUI(uiType);
            if (ShouldSkipUIChange(ui))
                return;

            DisableCurrentUI();
            ui.Enable();
            _stateManager.SetCurrentUI(ui);
        }

        private void DisableCurrentUI()
        {
            if (_stateManager.CurrentUI != null)
            {
                _stateManager.CurrentUI.Disable();
                _stateManager.SetCurrentUI(null);
            }
        }

        private bool ShouldSkipUIChange(BaseUI newUI)
        {
            return ReferenceEquals(_stateManager.CurrentUI, newUI);
        }
    }
}
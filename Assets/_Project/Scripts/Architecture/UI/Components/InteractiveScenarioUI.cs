using System;
using _Project.Scripts.Architecture.Core;
using _Project.Scripts.Architecture.Scenario.Core;
using _Project.Scripts.Architecture.UI.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Architecture.UI.Components
{
    public class InteractiveScenarioUI : BaseUI, IScenarioUI
    {
        [Header("Buttons")] [SerializeField] private ButtonStateLogic _interactButton;

        [SerializeField] private Button _infoButton;
        [SerializeField] private Button _editComponentButton;
        [SerializeField] private Button _graphicsButton;
        [SerializeField] private Button _closeButton;

        [Header("UI Elements")] [SerializeField]
        private InformationPanel _infoPanel;

        [SerializeField] private GraphicPanel _graphicPanel;
        [SerializeField] private ComponentEditorPanel _componentEditorPanel;

        private InteractiveScenario _currentScenario;
        private bool _isInteracting;
        [Inject] private SceneLoader _sceneLoader;

        [Inject] private IUIManager _uiManager;

        private void Reset()
        {
            _currentScenario = null;
            _infoPanel.ResetInfo();
            _graphicPanel.ResetGraphic();
            _componentEditorPanel.ResetComponentEditor();

            // Reset button states
            if (_interactButton != null)
                _interactButton.ResetButtonState();
        }

        private void Start()
        {
            ValidateComponents();
        }

        private void OnEnable()
        {
            SetupInfoButtonListeners();
            SetupGraphicsButtonListeners();
            SetupEditComponentButtonListeners();
            SetupInteractButtonListeners();
            SetupCloseButtonListeners();
        }

        private void OnDisable()
        {
            RemoveInfoButtonListeners();
            RemoveGraphicsButtonListeners();
            RemoveEditComponentButtonListeners();
            RemoveInteractButtonListeners();
            RemoveCloseButtonListeners();
            Reset();
        }

        public void SetScenario(BaseScenario scenario)
        {
            SetInteractiveScenario(scenario);
            if (_currentScenario != null)
            {
                _infoPanel.SetScenarioInfo(
                    _currentScenario.GetDisplayableInfo().Name,
                    _currentScenario.GetDisplayableInfo().Description
                );
                _graphicPanel.SetScenarioGraphic(_currentScenario);
                _componentEditorPanel.SetComponentEditor(_currentScenario);
            }
            else
            {
                Debug.LogError("Current scenario is null");
            }
        }

        private void ValidateComponents()
        {
            if (_interactButton == null)
                Debug.LogError("Interact button not assigned!", this);
            if (_infoButton == null)
                Debug.LogError("Info button not assigned!", this);
            if (_closeButton == null)
                Debug.LogError("Close button not assigned!", this);

            if (_infoPanel == null)
                Debug.LogError("Info panel not assigned!", this);
            if (_graphicPanel == null)
                Debug.LogError("Graphic panel not assigned!", this);
            if (_componentEditorPanel == null)
                Debug.LogError("Component editor panel not assigned!", this);
        }

        private void SetInteractiveScenario(BaseScenario scenario)
        {
            try
            {
                if (scenario is InteractiveScenario interactiveScenario)
                {
                    _currentScenario = interactiveScenario;
                }
                else
                {
                    throw new ArgumentException($"Invalid scenario type: {scenario.GetType()}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting scenario: {e.Message}");
                throw;
            }
        }

        private void StartSimulate()
        {
            if (_currentScenario != null)
            {
                _isInteracting = true;
                _currentScenario.OnInteractPlay();
            }
        }

        private void StopSimulate()
        {
            if (_currentScenario != null)
            {
                _isInteracting = false;
                _currentScenario.OnInteractStop();
            }
        }

        #region Setup Button Listeners

        private void SetupCloseButtonListeners()
        {
            _closeButton.onClick.AddListener(() => { _sceneLoader.LoadScene(0); }
            );
        }

        private void SetupInfoButtonListeners()
        {
            _infoButton.onClick.AddListener(ShowInfoPanel);
        }

        private void ShowInfoPanel()
        {
            if (_infoPanel != null)
            {
                _infoPanel.ShowPanel();
            }
        }

        private void SetupGraphicsButtonListeners()
        {
            _graphicsButton.onClick.AddListener(ShowGraphicsPanel);
        }

        private void ShowGraphicsPanel()
        {
            if (_graphicPanel != null)
            {
                _graphicPanel.ShowPanel();
            }
        }

        private void SetupEditComponentButtonListeners()
        {
            _editComponentButton.onClick.AddListener(ShowComponentEditorPanel);
        }

        private void ShowComponentEditorPanel()
        {
            if (!_isInteracting)
            {
                if (_componentEditorPanel != null)
                {
                    _componentEditorPanel.ShowPanel();
                }
            }
        }

        private void SetupInteractButtonListeners()
        {
            if (_interactButton != null)
            {
                _interactButton.OnFirstStateActivated.AddListener(StopSimulate);
                _interactButton.OnSecondStateActivated.AddListener(StartSimulate);
            }
        }

        #endregion

        #region Remove Button Listeners

        private void RemoveCloseButtonListeners()
        {
            _closeButton.onClick.RemoveAllListeners();
        }

        private void RemoveInfoButtonListeners()
        {
            _infoButton.onClick.RemoveAllListeners();
        }

        private void RemoveGraphicsButtonListeners()
        {
            _graphicsButton.onClick.RemoveAllListeners();
        }

        private void RemoveEditComponentButtonListeners()
        {
            _editComponentButton.onClick.RemoveAllListeners();
        }

        private void RemoveInteractButtonListeners()
        {
            if (_interactButton != null)
            {
                _interactButton.OnFirstStateActivated.RemoveAllListeners();
                _interactButton.OnSecondStateActivated.RemoveAllListeners();
            }
        }

        #endregion
    }
}
using System;
using _Project.Scripts.Architecture.Core;
using _Project.Scripts.Architecture.Scenario.Core;
using _Project.Scripts.Architecture.UI.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Architecture.UI.Components
{
    public class AnimationScenarioUI : BaseUI, IScenarioUI
    {
        [Header("Buttons")] [SerializeField] private ButtonStateLogic _interactButton;

        [SerializeField] private Button _infoButton;
        [SerializeField] private Button _closeButton;

        [Header("UI Elements")] [SerializeField]
        private InformationPanel _infoPanel;

        private AnimationScenario _currentScenario;
        [Inject] private SceneLoader _sceneLoader;

        [Inject] private IUIManager _uiManager;

        private void Reset()
        {
            _currentScenario = null;
            _infoPanel.ResetInfo();

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
            SetupInteractButtonListeners();
            SetupCloseButtonListeners();
        }

        private void OnDisable()
        {
            RemoveInfoButtonListeners();
            RemoveInteractButtonListeners();
            RemoveCloseButtonListeners();
            Reset();
        }

        public void SetScenario(BaseScenario scenario)
        {
            SetAnimationScenario(scenario);
            if (_currentScenario != null)
            {
                _infoPanel.SetScenarioInfo(
                    _currentScenario.GetDisplayableInfo().Name,
                    _currentScenario.GetDisplayableInfo().Description
                );
                if (_currentScenario.IsAutoPlay)
                {
                    Debug.Log("AutoPlay: " + _currentScenario.IsAutoPlay);
                    _interactButton.SetButtonState(ButtonState.SecondState);
                }
            }
        }

        protected void SetAnimationScenario(BaseScenario scenario)
        {
            try
            {
                if (scenario is AnimationScenario animationScenario)
                {
                    _currentScenario = animationScenario;
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
        }

        private void StartSimulate()
        {
            if (_currentScenario != null)
            {
                _currentScenario.OnInteractPlay();
            }
        }

        private void StopSimulate()
        {
            if (_currentScenario != null)
            {
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
            _infoButton.onClick.AddListener(_infoPanel.ShowPanel);
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
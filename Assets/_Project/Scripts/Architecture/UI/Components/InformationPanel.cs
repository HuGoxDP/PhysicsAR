using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Architecture.UI.Components
{
    public sealed class InformationPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scenarioName;
        [SerializeField] private TextMeshProUGUI _scenarioDescription;

        [Header("Buttons")] [SerializeField] private Button _closeButton;

        [SerializeField] private Button _outboundButton;

        private void Start()
        {
            ValidateComponents();

            _closeButton.onClick.AddListener(HideInfoPanel);
            _outboundButton.onClick.AddListener(HideInfoPanel);
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
            _outboundButton.onClick.RemoveAllListeners();
        }

        public void SetScenarioInfo(string scenarioName, string scenarioDescription)
        {
            _scenarioName.text = scenarioName;
            _scenarioDescription.text = scenarioDescription;
        }

        public void ShowPanel()
        {
            gameObject.SetActive(true);
        }

        private void ValidateComponents()
        {
            if (_scenarioName == null)
                Debug.LogError("Scenario name not assigned!", this);
            if (_scenarioDescription == null)
                Debug.LogError("Scenario description not assigned!", this);
            if (_closeButton == null)
                Debug.LogError("Close button not assigned!", this);
            if (_outboundButton == null)
                Debug.LogError("Outbound button not assigned!", this);
        }

        private void HideInfoPanel()
        {
            gameObject.SetActive(false);
        }

        public void ResetInfo()
        {
            _scenarioName.text = "Scenario Name";
            _scenarioDescription.text = "Scenario Description";
        }
    }
}
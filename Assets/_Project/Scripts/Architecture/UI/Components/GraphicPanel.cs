using System.Collections.Generic;
using _Project.Scripts.Architecture.Scenario.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Architecture.UI.Components
{
    public sealed class GraphicPanel : MonoBehaviour
    {
        [Header("Buttons")] [SerializeField] private Button _closeButton;

        [SerializeField] private Button _outboundButton;

        [Header("UI Elements")] [SerializeField]
        private RectTransform _container;

        [Header("UI prefabs")] [SerializeField]
        private GameObject _graphicalComponentPrefab;

        private readonly List<GameObject> _collection = new();

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

        public void ShowPanel()
        {
            gameObject.SetActive(true);
        }

        private void ValidateComponents()
        {
            if (_closeButton == null)
                Debug.LogError("Close button not assigned!", this);
            if (_outboundButton == null)
                Debug.LogError("Outbound button not assigned!", this);
            if (_container == null)
                Debug.LogError("Container not assigned!", this);
            if (_graphicalComponentPrefab == null)
                Debug.LogError("Graphical component prefab not assigned!", this);
        }

        private void HideInfoPanel()
        {
            gameObject.SetActive(false);
        }

        public void ResetGraphic()
        {
            if (_collection != null)
            {
                _container.sizeDelta = new Vector2(_container.sizeDelta.x, 300f);
                foreach (var component in _collection)
                {
                    Destroy(component);
                }
            }
        }

        public void SetScenarioGraphic(InteractiveScenario currentScenario)
        {
            int rectHeightPlus = 0;
            foreach (var component in currentScenario.GetDisplayableComponents())
            {
                var graphic = Instantiate(_graphicalComponentPrefab, _container);
                var graphicalComponent = graphic.GetComponent<GraphicalComponentUI>();
                if (graphicalComponent != null)
                {
                    graphicalComponent.SetComponent(component);
                    _collection.Add(graphic);
                    rectHeightPlus += 42;
                }
                else
                {
                    Debug.LogError($"Graphical component not found in prefab: {graphic.name}", this);
                }
            }

            _container.sizeDelta = new Vector2(_container.sizeDelta.x, 50 + rectHeightPlus);
        }
    }
}
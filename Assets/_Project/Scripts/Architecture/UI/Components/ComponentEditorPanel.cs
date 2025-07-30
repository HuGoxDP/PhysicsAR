using System.Collections.Generic;
using _Project.Scripts.Architecture.Scenario.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Architecture.UI.Components
{
    public sealed class ComponentEditorPanel : MonoBehaviour
    {
        [Header("Buttons")] [SerializeField] private Button _closeButton;

        [SerializeField] private Button _outboundButton;

        [Header("UI Elements")] [SerializeField]
        private RectTransform _container;

        [Header("UI prefabs")] [SerializeField]
        private GameObject _inputFieldPrefab;

        [SerializeField] private Transform _dropdownPrefab;

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
            if (_inputFieldPrefab == null)
                Debug.LogError("Input field prefab not assigned!", this);
            if (_dropdownPrefab == null)
                Debug.LogError("Dropdown prefab not assigned!", this);
        }

        private void HideInfoPanel()
        {
            gameObject.SetActive(false);
        }

        public void ResetComponentEditor()
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


        public void SetComponentEditor(InteractiveScenario currentScenario)
        {
            int rectHeightPlus = 0;

            foreach (var editableComponent in currentScenario.GetEditableComponents())
            {
                if (editableComponent.ComponentType == ObservableFieldComponentType.InputField)
                {
                    var inputField = Instantiate(_inputFieldPrefab, _container);
                    var inputFieldComponent = inputField.GetComponent<InputFieldEditableUI>();
                    if (inputFieldComponent != null)
                    {
                        inputFieldComponent.SetEditableComponent(editableComponent);
                        _collection.Add(inputField);
                    }
                }
                else if (editableComponent.ComponentType == ObservableFieldComponentType.Dropdown)
                {
                    var dropdown = Instantiate(_dropdownPrefab, _container);
                    var dropdownComponent = dropdown.GetComponent<DropDownEditableUI>();
                    if (dropdownComponent != null)
                    {
                        dropdownComponent.SetEditableComponent(editableComponent);
                        _collection.Add(dropdown.gameObject);
                        rectHeightPlus += 42;
                    }
                }
                else
                {
                    Debug.LogError($"Unsupported component type: {editableComponent.ComponentType}");
                }
            }

            _container.sizeDelta = new Vector2(_container.sizeDelta.x, 50 + rectHeightPlus);
        }
    }
}
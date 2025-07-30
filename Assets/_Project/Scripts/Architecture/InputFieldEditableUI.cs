using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Architecture
{
    public class InputFieldEditableUI : BaseEditableUI<TMP_InputField>
    {
        private static readonly Dictionary<Type, TMP_InputField.ContentType> ContentTypeMap =
            new()
            {
                { typeof(int), TMP_InputField.ContentType.IntegerNumber },
                { typeof(float), TMP_InputField.ContentType.DecimalNumber },
                { typeof(string), TMP_InputField.ContentType.Standard }
            };

        [SerializeField] private TextMeshProUGUI _placeholderText;

        protected override void RegisterEvents()
        {
            _uiComponent.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void UnregisterEvents()
        {
            _uiComponent.onValueChanged.RemoveAllListeners();
        }

        protected override void InitializeTypeHandlers()
        {
            _typeHandlers = new Dictionary<Type, Action<IObservableFieldComponent>>
            {
                { typeof(int), component => HandleNumericField<int>(component) },
                { typeof(float), component => HandleNumericField<float>(component) },
                { typeof(string), component => HandleStringField(component) }
            };
        }

        private void HandleNumericField<T>(IObservableFieldComponent component) where T : struct, IConvertible
        {
            var typedComponent = component as ObservableFieldComponent<T>;
            if (typedComponent == null) return;

            _uiComponent.contentType = ContentTypeMap.TryGetValue(typeof(T), out var contentType)
                ? contentType
                : TMP_InputField.ContentType.Standard;

            _uiComponent.SetTextWithoutNotify(typedComponent.Value.ToString());
            _placeholderText.text = "Enter Number";
        }

        private void HandleStringField(IObservableFieldComponent component)
        {
            var typedComponent = component as ObservableFieldComponent<string>;
            if (typedComponent == null) return;

            _uiComponent.contentType = TMP_InputField.ContentType.Standard;
            _uiComponent.SetTextWithoutNotify(typedComponent.Value ?? string.Empty);
            _placeholderText.text = "Enter Text";
        }

        private void OnValueChanged(string newValue)
        {
            if (_currentObservableFieldComponent == null) return;

            Type fieldType = _currentObservableFieldComponent.FieldType;

            try
            {
                if (fieldType == typeof(int) && int.TryParse(newValue, out var intValue))
                {
                    ((ObservableFieldComponent<int>)_currentObservableFieldComponent).TrySetValue(intValue);
                }
                else if (fieldType == typeof(float) && float.TryParse(newValue, out var floatValue))
                {
                    ((ObservableFieldComponent<float>)_currentObservableFieldComponent).TrySetValue(floatValue);
                }
                else if (fieldType == typeof(string))
                {
                    ((ObservableFieldComponent<string>)_currentObservableFieldComponent).TrySetValue(newValue);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating value: {e.Message}");
            }
        }
    }
}
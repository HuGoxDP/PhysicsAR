using System;
using System.Collections.Generic;
using HuGox.Utils;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Architecture
{
    public class DropDownEditableUI : BaseEditableUI<TMP_Dropdown>
    {
        // Type registry with dynamic handler creation
        private new readonly Dictionary<Type, IDropdownTypeHandler> _typeHandlers = new();

        protected override void InitializeTypeHandlers()
        {
            // Register common types explicitly for performance
            RegisterHandler<int>();
            RegisterHandler<float>();
            RegisterHandler<string>();
            RegisterHandler<bool>();
        }

        // Method to register a type handler
        private void RegisterHandler<T>()
        {
            if (!_typeHandlers.ContainsKey(typeof(T)))
            {
                _typeHandlers.Add(typeof(T), new DropdownTypeHandler<T>());
            }
        }

        // Method to get or create a handler for any type
        private IDropdownTypeHandler GetHandler(Type type)
        {
            if (_typeHandlers.TryGetValue(type, out var handler))
            {
                return handler;
            }

            // Dynamically create handler for unknown type
            handler = CreateHandlerForType(type);
            _typeHandlers.Add(type, handler);
            return handler;
        }

        // Create handler dynamically using reflection
        private IDropdownTypeHandler CreateHandlerForType(Type type)
        {
            Type handlerType = typeof(DropdownTypeHandler<>).MakeGenericType(type);
            return Activator.CreateInstance(handlerType) as IDropdownTypeHandler;
        }

        protected override void RegisterEvents()
        {
            _uiComponent.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void UnregisterEvents()
        {
            _uiComponent.onValueChanged.RemoveAllListeners();
        }

        public override void SetEditableComponent(IObservableFieldComponent component)
        {
            _fieldName.SetText(component.DisplayName);
            _currentObservableFieldComponent = component;

            var handler = GetHandler(component.FieldType);
            handler.Setup(component, _uiComponent);
        }

        private void OnValueChanged(int valueIndex)
        {
            if (_currentObservableFieldComponent == null) return;

            try
            {
                var handler = GetHandler(_currentObservableFieldComponent.FieldType);
                handler.HandleValueChanged(_currentObservableFieldComponent, valueIndex);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating dropdown value: {e.Message}");
            }
        }

        // Interface for type-specific handlers
        private interface IDropdownTypeHandler
        {
            void Setup(IObservableFieldComponent component, TMP_Dropdown dropdown);
            void HandleValueChanged(IObservableFieldComponent component, int valueIndex);
        }

        // Generic implementation of type handler
        private class DropdownTypeHandler<T> : IDropdownTypeHandler
        {
            public void Setup(IObservableFieldComponent component, TMP_Dropdown dropdown)
            {
                var typedComponent = component as ObservableFieldComponent<T>;
                if (typedComponent == null) return;

                var options = new List<TMP_Dropdown.OptionData>(typedComponent.AvailableOptions.Count);
                foreach (var option in typedComponent.AvailableOptions)
                {
                    options.Add(new TMP_Dropdown.OptionData(option.ToString()));
                }

                dropdown.SetValueWithoutNotify(0);
                dropdown.ClearOptions();
                dropdown.AddOptions(options);

                int selectedIndex = typedComponent.AvailableOptions.IndexOf(typedComponent.Value);
                if (selectedIndex >= 0)
                {
                    dropdown.SetValueWithoutNotify(selectedIndex);
                }
            }

            public void HandleValueChanged(IObservableFieldComponent component, int valueIndex)
            {
                var typedComponent = component as ObservableFieldComponent<T>;
                if (typedComponent == null || valueIndex < 0 || valueIndex >= typedComponent.AvailableOptions.Count)
                    return;

                typedComponent.TrySetValue(typedComponent.AvailableOptions[valueIndex]);
            }
        }
    }
}
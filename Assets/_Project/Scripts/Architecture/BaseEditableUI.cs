using System;
using System.Collections.Generic;
using _Project.Scripts.Architecture.Scenario.FrictionScenario;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Architecture
{
    public abstract class BaseEditableUI<T> : MonoBehaviour, IObservableFieldComponentEditableUI where T : Selectable
    {
        [SerializeField] protected TextMeshProUGUI _fieldName;
        [SerializeField] protected T _uiComponent;
        protected IObservableFieldComponent _currentObservableFieldComponent;

        protected Dictionary<Type, Action<IObservableFieldComponent>> _typeHandlers;

        protected virtual void Awake()
        {
            RegisterEvents();
        }

        protected virtual void OnDestroy()
        {
            UnregisterEvents();
        }

        public virtual void SetEditableComponent(IObservableFieldComponent observableFieldComponent)
        {
            InitializeTypeHandlers();
            _fieldName.SetText(observableFieldComponent.DisplayName);
            _currentObservableFieldComponent = observableFieldComponent;
            if (_typeHandlers.TryGetValue(observableFieldComponent.FieldType, out var handler))
            {
                handler(observableFieldComponent);
            }
            else
            {
                Debug.LogError($"No handler found for type {observableFieldComponent.FieldType}");
            }
        }

        protected abstract void RegisterEvents();
        protected abstract void UnregisterEvents();
        protected abstract void InitializeTypeHandlers();
    }
}
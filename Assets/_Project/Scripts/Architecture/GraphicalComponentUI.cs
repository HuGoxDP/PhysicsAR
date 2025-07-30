using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Architecture
{
    public class GraphicalComponentUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _displayableName;
        [SerializeField] private TextMeshProUGUI _displayableInfo;


        private readonly Dictionary<Type, IGraphicalTypeHandler> _typeHandlers = new();

        private IObservableFieldComponent _observableFieldComponent;

        private void OnDestroy()
        {
            foreach (var handler in _typeHandlers)
            {
                handler.Value.Dispose();
            }
        }

        public void SetComponent(IObservableFieldComponent observableFieldComponent)
        {
            _observableFieldComponent = observableFieldComponent;
            _displayableName.text = _observableFieldComponent.DisplayName;
            var handler = GetHandler(observableFieldComponent.FieldType);
            handler.Setup(observableFieldComponent, _displayableInfo);
        }

        private IGraphicalTypeHandler GetHandler(Type type)
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

        private IGraphicalTypeHandler CreateHandlerForType(Type type)
        {
            Type handlerType = typeof(GraphicalTypeHandler<>).MakeGenericType(type);
            return Activator.CreateInstance(handlerType) as IGraphicalTypeHandler;
        }

        // Interface for type-specific handlers
        private interface IGraphicalTypeHandler : IDisposable
        {
            void Setup(IObservableFieldComponent component, TextMeshProUGUI displayableInfo);
        }

        private class GraphicalTypeHandler<T> : IGraphicalTypeHandler
        {
            CompositeDisposable _disposable = new CompositeDisposable();

            public void Setup(IObservableFieldComponent component, TextMeshProUGUI displayableInfo)
            {
                var typedComponent = component as ObservableFieldComponent<T>;
                if (typedComponent == null) return;

                typedComponent.ValueObservable.Subscribe(_ =>
                        {
                            displayableInfo.text = typedComponent.GetFormattedValue();
                        }
                    )
                    .AddTo(_disposable);
            }

            public void Dispose()
            {
                _disposable.Dispose();
            }
        }
    }
}
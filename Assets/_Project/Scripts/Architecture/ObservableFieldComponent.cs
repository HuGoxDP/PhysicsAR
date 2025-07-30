using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace _Project.Scripts.Architecture
{
    public interface IObservableFieldComponent : IDisposable
    {
        string DisplayName { get; }
        ObservableFieldComponentType ComponentType { get; }

        Type FieldType { get; }
    }

    [Serializable]
    public class ObservableFieldComponent<T> : IObservableFieldComponent
    {
        private readonly List<IValueRestriction<T>> _restrictions = new List<IValueRestriction<T>>();
        private readonly BehaviorSubject<T> _valueSubject;
        private string _format;
        private Lazy<List<T>> _lazyAvailableOptions;
        private string _unit;

        public ObservableFieldComponent(T initialValue,
            ObservableFieldComponentType componentType, string displayName,
            string format = null, string unit = "")
        {
            ComponentType = componentType;
            DisplayName = displayName;
            _valueSubject = new BehaviorSubject<T>(initialValue);
            _format = format;
            _unit = unit;
            FieldType = typeof(T);

            _lazyAvailableOptions = new Lazy<List<T>>(() => new List<T>());
        }

        public Observable<T> ValueObservable => _valueSubject;
        public T Value => _valueSubject.Value;

        public IReadOnlyList<T> AvailableOptions => _lazyAvailableOptions.Value;
        public ObservableFieldComponentType ComponentType { get; private set; }
        public Type FieldType { get; private set; }
        public string DisplayName { get; private set; }

        public void Dispose()
        {
            _valueSubject?.Dispose();
        }

        public string GetFormattedValue()
        {
            if (string.IsNullOrEmpty(_format))
            {
                return Value.ToString() + (!string.IsNullOrEmpty(_unit) ? " " + _unit : "");
            }

            return string.Format(_format, Value) + (!string.IsNullOrEmpty(_unit) ? " " + _unit : "");
        }

        public void SetAvailableOptions(IEnumerable<T> options)
        {
            if (ComponentType != ObservableFieldComponentType.Dropdown)
            {
                Debug.LogWarning($"Setting options for non-dropdown component +{DisplayName}");
            }

            if (!_lazyAvailableOptions.IsValueCreated)
            {
                _lazyAvailableOptions = new Lazy<List<T>>(options.ToList);
            }
            else
            {
                _lazyAvailableOptions.Value.Clear();
                _lazyAvailableOptions.Value.AddRange(options);
            }
        }

        public void AddRestriction(IValueRestriction<T> restriction)
        {
            if (restriction == null)
            {
                Debug.LogError("Restriction is null");
                return;
            }

            _restrictions.Add(restriction);
        }

        public bool TrySetValue(T newValue)
        {
            foreach (var valueRestriction in _restrictions)
            {
                if (!valueRestriction.IsValid(newValue))
                {
                    Debug.LogError(valueRestriction.GetErrorMessage(newValue));
                    return false;
                }
            }

            _valueSubject.OnNext(newValue);
            return true;
        }
    }
}
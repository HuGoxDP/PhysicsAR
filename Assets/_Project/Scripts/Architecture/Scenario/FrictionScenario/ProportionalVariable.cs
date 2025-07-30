using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.FrictionScenario
{
    public interface IProportionalVariable
    {
        float CurrentValue { get; set; }
        float NormalizedValue { get; }
        void SetFromNormalized(float normalizedValue);
    }

    public class ProportionalVariable : IProportionalVariable
    {
        private readonly float _maxValue;
        private readonly float _minValue;
        private float _currentValue;

        public ProportionalVariable(float maxValue, float minValue, float currentValue)
        {
            // Проверка и обмен значений, если min > max
            if (minValue > maxValue)
            {
                (minValue, maxValue) = (maxValue, minValue);
                Debug.LogWarning("Минимальное значение было больше максимального. Значения поменяны местами.");
            }

            _maxValue = maxValue;
            _minValue = minValue;
            _currentValue = Mathf.Clamp(currentValue, _minValue, _maxValue);
            ;
        }

        public float CurrentValue
        {
            get => _currentValue;
            set => _currentValue = Mathf.Clamp(value, _minValue, _maxValue);
        }

        public float NormalizedValue => (_currentValue - _minValue) / (_maxValue - _minValue);

        public void SetFromNormalized(float normalizedValue)
        {
            normalizedValue = Mathf.Clamp01(normalizedValue);
            _currentValue = Mathf.Lerp(_minValue, _maxValue, normalizedValue);
        }
    }
}
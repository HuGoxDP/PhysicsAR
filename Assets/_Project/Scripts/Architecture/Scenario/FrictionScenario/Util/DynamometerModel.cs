using System;
using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.FrictionScenario.Util
{
    public class DynamometerModel
    {
        private const float MAXVALUE = 10f;
        private const float MINVALUE = 0f;

        private float _value;

        public Action<float> OnValueChanged = delegate { };


        public DynamometerModel(float value)
        {
            _value = Mathf.Clamp(value, MINVALUE, MAXVALUE);
        }

        public float Value
        {
            get => _value;
            set
            {
                _value = Mathf.Clamp(value, MINVALUE, MAXVALUE);
                OnValueChanged?.Invoke(_value);
            }
        }

        public void Reset() => Value = 0f;
    }
}
using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.FrictionScenario
{
    public class ProportionalVariableManager : MonoBehaviour
    {
        [Header("Proportional A settings")] [SerializeField]
        float _maxValueA = 100f;

        [SerializeField] float _minValueA = 0;
        [SerializeField] float _currentValueA = 0f;

        [Header("Proportional B settings")] [SerializeField]
        float _maxValueB = 100f;

        [SerializeField] float _minValueB = 0;
        [SerializeField] float _currentValueB = 0f;

        private IProportionalVariable _variableA;
        private IProportionalVariable _variableB;

        private void Awake()
        {
            _variableA = new ProportionalVariable(_maxValueA, _minValueA, _currentValueA);
            _variableB = new ProportionalVariable(_maxValueB, _minValueB, _currentValueB);
        }

        //Update the value of variable B based on variable A
        public void UpdateFromA()
        {
            float normalizedA = _variableA.NormalizedValue;
            _variableB.SetFromNormalized(normalizedA);
        }

        // Update the value of variable A based on variable B
        public void UpdateFromB()
        {
            float normalizedB = _variableB.NormalizedValue;
            _variableA.SetFromNormalized(normalizedB);
        }

        // Set the value of variable A and update variable B
        public void SetVariableA(float value)
        {
            _variableA.CurrentValue = value;
            UpdateFromA();
        }

        // Set the value of variable B and update variable A
        public void SetVariableB(float value)
        {
            _variableB.CurrentValue = value;
            UpdateFromB();
        }

        // Get the value of variable A
        public float GetVariableA()
        {
            return _variableA.CurrentValue;
        }

        // Get the value of variable B
        public float GetVariableB()
        {
            return _variableB.CurrentValue;
        }
    }
}
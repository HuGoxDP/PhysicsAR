using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.FrictionScenario.Util
{
    public class DynamometerTester : MonoBehaviour
    {
        [SerializeField] private DynamometerController _dynamometerController;

        private void Start()
        {
            if (_dynamometerController == null)
            {
                Debug.LogError("DynamometerController is not assigned!");
                return;
            }

            // Example usage
            _dynamometerController.SetValue(5f);
            Debug.Log($"Current Dynamometer Value: {_dynamometerController.GetValue()}");
        }

        [Button]
        private void ResetDynamometer()
        {
            _dynamometerController.SetValue(0f);
            Debug.Log("Dynamometer reset to 0");
        }

        [Button]
        private void IncreaseValue(float increment)
        {
            float newValue = _dynamometerController.GetValue() + increment;
            _dynamometerController.SetValue(newValue);
            Debug.Log($"Dynamometer value increased by {increment}, new value: {_dynamometerController.GetValue()}");
        }

        [Button]
        private void DecreaseValue(float decrement)
        {
            float newValue = _dynamometerController.GetValue() - decrement;
            _dynamometerController.SetValue(newValue);
            Debug.Log($"Dynamometer value decreased by {decrement}, new value: {_dynamometerController.GetValue()}");
        }
    }
}
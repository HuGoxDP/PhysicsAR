using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.FrictionScenario.Util
{
    [RequireComponent(typeof(DynamometerView))]
    public class DynamometerController : MonoBehaviour
    {
        private DynamometerModel _model;
        private DynamometerView _view;

        private void Awake()
        {
            _model = new DynamometerModel(0f);
            _view = GetComponent<DynamometerView>();
            _model.OnValueChanged += UpdateDynamometerView;
        }

        private void Reset()
        {
            _model.Reset();
        }

        private void OnDestroy()
        {
            _model.OnValueChanged -= UpdateDynamometerView;
        }

        private void UpdateDynamometerView(float value)
        {
            _view.SetDynamometerValue(value);
        }

        public void SetHookAnimation()
        {
            _view.SetHookAnimationMode();
        }

        public void SetBodyAnimation()
        {
            _view.SetBodyAnimationMode();
        }

        public void SetValue(float value)
        {
            _model.Value = value;
        }

        public float GetValue()
        {
            return _model.Value;
        }
    }
}
using R3;
using UnityEngine;

namespace _Project.Scripts.Architecture
{
    public class ForceArrowController : MonoBehaviour
    {
        [SerializeField] private ForceArrow _forceArrow;
        private ObservableFieldComponent<float> _forceValue;

        public void Initialize(ObservableFieldComponent<float> forceComponent)
        {
            _forceValue = forceComponent;
            _forceValue.ValueObservable.Subscribe(UpdateArrowForce)
                .AddTo(this);
            UpdateArrowForce(_forceValue.Value);
        }

        public void SetMaxSize(float maxSize)
        {
            if (_forceArrow != null)
            {
                _forceArrow.SetMaxSize(maxSize);
            }
        }

        private void UpdateArrowForce(float force)
        {
            if (_forceArrow != null)
            {
                _forceArrow.SetForce(force);
            }
        }

        public void SetForceText(string text)
        {
            if (_forceArrow != null)
            {
                _forceArrow.SetForceText(text);
            }
        }

        public void SetForceSymbol(string symbol)
        {
            if (_forceArrow != null)
            {
                _forceArrow.SetForceSymbol(symbol);
            }
        }
    }
}
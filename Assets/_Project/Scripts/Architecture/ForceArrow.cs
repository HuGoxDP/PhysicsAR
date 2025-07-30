using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Architecture
{
    public class ForceArrow : MonoBehaviour
    {
        [Header("Text Properties")] [SerializeField]
        private TextMeshProUGUI _forceSymbol;

        [SerializeField] private TextMeshProUGUI _forceText;

        [Header("Arrow Properties")] [SerializeField]
        private Image _arrowBodyRenderer;

        [SerializeField] private Image _arrowStartRenderer;
        [SerializeField] private Image _arrowHeadRenderer;
        [SerializeField] private RectTransform _arrowRectTransform;
        [SerializeField] private Color _arrowColor = Color.red;

        [Header("Scale Settings")] [SerializeField]
        private float _baseForceValue = 0f;

        [SerializeField] private float _scaleFactor = 0.01f;


        private float _currentForce;
        private float _maxSize = 5f;

        private float _minSize = 0f;

        private void Start()
        {
            if (_arrowBodyRenderer)
            {
                _arrowBodyRenderer.color = _arrowColor;
            }
            else
            {
                Debug.LogWarning("No arrow renderer attached");
            }

            if (_arrowHeadRenderer)
            {
                _arrowHeadRenderer.color = _arrowColor;
            }
            else
            {
                Debug.LogWarning("No arrow renderer attached");
            }

            if (_arrowStartRenderer)
            {
                _arrowStartRenderer.color = _arrowColor;
            }
            else
            {
                Debug.LogWarning("No arrow renderer attached");
            }

            SetForce(_baseForceValue);
        }

        public void SetMaxSize(float maxSize)
        {
            _maxSize = maxSize;
        }

        public void SetForce(float force)
        {
            _currentForce = force;
            UpdateArrowVisual();
        }

        public void SetForceText(string text)
        {
            if (_forceText)
            {
                _forceText.text = text;
            }
            else
            {
                Debug.LogWarning("No force text attached");
            }
        }

        public void SetForceSymbol(string symbol)
        {
            if (_forceSymbol)
            {
                _forceSymbol.text = symbol;
            }
            else
            {
                Debug.LogWarning("No force symbol attached");
            }
        }

        private void UpdateArrowVisual()
        {
            // Calculate arrow length based on force
            float normalizedLength = Mathf.Clamp(_currentForce * _scaleFactor, _minSize, _maxSize);

            // Update arrow body scale and position
            float arrowHeight = 0.3f;
            _arrowRectTransform.sizeDelta = new Vector2(normalizedLength, arrowHeight);
            if (_arrowRectTransform.rect.width < 0.01f)
            {
                _arrowBodyRenderer.gameObject.SetActive(false);
                _arrowHeadRenderer.gameObject.SetActive(false);
            }
            else
            {
                _arrowBodyRenderer.gameObject.SetActive(true);
                _arrowHeadRenderer.gameObject.SetActive(true);
            }
        }
    }
}
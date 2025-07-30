using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.Scripts.Architecture
{
    public enum ButtonState
    {
        FirstState,
        SecondState
    }

    [RequireComponent(typeof(Button))]
    public class ButtonStateLogic : MonoBehaviour
    {
        [Header("Button Setup")] [SerializeField]
        private Sprite _firstStateIcon;

        [SerializeField] private Sprite _secondStateIcon;

        [Header("Button State")] [SerializeField]
        private ButtonState _initialButtonState = ButtonState.FirstState;

        [Header("Events")] public UnityEvent OnFirstStateActivated = new();

        public UnityEvent OnSecondStateActivated = new();
        private Button _button;
        private ButtonState _buttonState;

        public ButtonState CurrentState => _buttonState;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonState = _initialButtonState;
            UpdateButtonSprite();
        }

        private void Start()
        {
            _button.onClick.AddListener(ToggleButtonState);
        }


        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(ToggleButtonState);

            OnFirstStateActivated.RemoveAllListeners();
            OnSecondStateActivated.RemoveAllListeners();
        }

        private void OnValidate()
        {
            _button ??= GetComponent<Button>();

            if (_button == null || _button.image == null)
                return;
            _buttonState = _initialButtonState;
            UpdateButtonSprite();
        }

        /// <summary>
        /// Sets the button to the specified state and updates its appearance.
        /// </summary>
        public void SetButtonState(ButtonState state, bool withInvoke = true)
        {
            if (state == ButtonState.FirstState)
                SetFirstState(withInvoke);
            else
                SetSecondState(withInvoke);
        }

        public void ResetButtonState()
        {
            SetButtonState(_initialButtonState, false);
        }

        private void UpdateButtonSprite()
        {
            if (_button?.image == null)
                return;

            _button.image.sprite = _buttonState == ButtonState.FirstState ? _firstStateIcon : _secondStateIcon;
        }

        /// <summary>
        /// Sets the button to the first state, updates the sprite,
        /// and invokes the OnSecondStateChanged event.
        /// </summary>
        private void SetFirstState(bool withInvoke = true)
        {
            _buttonState = ButtonState.FirstState;
            UpdateButtonSprite();
            if (withInvoke)
                OnFirstStateActivated?.Invoke();
        }

        /// <summary>
        /// Sets the button to the second state, updates the sprite,
        /// and invokes the OnFirstStateChanged event.
        /// </summary>
        private void SetSecondState(bool withInvoke = true)
        {
            _buttonState = ButtonState.SecondState;
            UpdateButtonSprite();
            if (withInvoke)
                OnSecondStateActivated?.Invoke();
        }

        /// <summary>
        /// Toggles between the first and second states when the button is clicked.
        /// </summary>
        private void ToggleButtonState()
        {
            if (_buttonState == ButtonState.FirstState)
                SetSecondState();
            else
                SetFirstState();
        }
    }
}
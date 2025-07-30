using _Project.Scripts.Architecture.Scenario.Core;
using R3;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.FrictionScenario
{
    public sealed class InteractiveScenarioStaticFrictionOvercoming : InteractiveScenario
    {
        [SerializeField] private TextMeshProUGUI _boxMaterialText;
        [SerializeField] private TextMeshProUGUI _groundMaterialText;
        [SerializeField] private Rigidbody2D _boxRigidbody;

        [SerializeField] private ForceArrowController _appliedForceArrow;

        [SerializeField] private ForceArrowController _staticFrictionArrow;

        // Forces
        private ObservableFieldComponent<float> _appliedForce;

        // Materials
        private ObservableFieldComponent<FrictionMaterial> _boxMaterial;
        private ObservableFieldComponent<float> _boxVelocity;

        // Physics
        private ObservableFieldComponent<float> _boxWeight;
        private ObservableFieldComponent<float> _currentAppliedForce;

        private ObservableFieldComponent<float> _currentStaticFriction;

        private CompositeDisposable _disposables = new();
        private ObservableFieldComponent<float> _gravity;
        private ObservableFieldComponent<FrictionMaterial> _groundMaterial;
        private Vector3 _initialBoxPosition;
        private Quaternion _initialBoxRotation;
        private ObservableFieldComponent<float> _kineticFrictionCoefficient;
        private ObservableFieldComponent<float> _kineticFrictionForce;
        private ObservableFieldComponent<float> _normalForce;
        private ObservableFieldComponent<float> _staticFrictionCoefficient;
        private ObservableFieldComponent<float> _staticFrictionMax;

        private void Awake()
        {
            _initialBoxPosition = _boxRigidbody.transform.position;
            _initialBoxRotation = _boxRigidbody.transform.rotation;

            InitializeComponents();
            UpdateMaterialTexts();
            AddAllComponentsToObservables();
        }

        private void Start()
        {
            _appliedForceArrow.Initialize(_currentAppliedForce);
            _staticFrictionArrow.Initialize(_currentStaticFriction);
        }

        private void OnDestroy()
        {
            _disposables.Clear();
            _appliedForceArrow.gameObject.SetActive(false);
            _staticFrictionArrow.gameObject.SetActive(false);
        }

        public override void OnInteractPlay()
        {
            Debug.Log("Starting simulation");
            ResetPhysicsStateImmediate();

            Observable.EveryUpdate(UnityFrameProvider.FixedUpdate)
                .Subscribe(_ => UpdatePhysics())
                .AddTo(_disposables);
        }

        private void ResetPhysicsStateImmediate()
        {
            _boxRigidbody.linearVelocity = Vector2.zero;
            _boxRigidbody.transform.position = _initialBoxPosition;
            _boxRigidbody.transform.rotation = _initialBoxRotation;
            _currentStaticFriction.TrySetValue(0);
            _currentAppliedForce.TrySetValue(0);
            _boxVelocity.TrySetValue(0);
            _appliedForceArrow.gameObject.SetActive(true);
            _staticFrictionArrow.gameObject.SetActive(true);
            _staticFrictionArrow.SetForceText($"тертя сп");
        }

        public override void OnInteractStop()
        {
            Debug.Log("Stopping simulation");
            _disposables.Clear();
            ResetPhysicsStateImmediate();
        }

        private void InitializeComponents()
        {
            // Ініціалізація полів
            _boxMaterial = new ObservableFieldComponent<FrictionMaterial>(
                FrictionMaterialRegistry.Instance.GetById("wood"),
                ObservableFieldComponentType.None,
                "Матеріал коробки"
            );

            _boxVelocity = new ObservableFieldComponent<float>(
                0f,
                ObservableFieldComponentType.None,
                "Швидкість коробки",
                "{0:0.00}",
                "м/с"
            );

            _groundMaterial = new ObservableFieldComponent<FrictionMaterial>(
                FrictionMaterialRegistry.Instance.GetById("concrete"),
                ObservableFieldComponentType.None,
                "Матеріал підлоги"
            );

            _staticFrictionCoefficient = new ObservableFieldComponent<float>(
                FrictionBaseData.GetStaticCoefficient(_boxMaterial.Value, _groundMaterial.Value),
                ObservableFieldComponentType.None,
                "Коефіцієнт тертя спокою",
                "{0:0.00}"
            );

            _kineticFrictionCoefficient = new ObservableFieldComponent<float>(
                FrictionBaseData.GetKineticCoefficient(_boxMaterial.Value, _groundMaterial.Value),
                ObservableFieldComponentType.None,
                "Коефіцієнт тертя ковзання",
                "{0:0.00}"
            );
            _boxWeight = new ObservableFieldComponent<float>(
                100f,
                ObservableFieldComponentType.None,
                "Вага (кг)",
                "{0:0.#}",
                "кг"
            );

            _gravity = new ObservableFieldComponent<float>(
                -Physics2D.gravity.y,
                ObservableFieldComponentType.None,
                "Гравітація",
                "{0:0.00}",
                "м/с²"
            );

            _normalForce = new ObservableFieldComponent<float>(
                _boxWeight.Value * _gravity.Value,
                ObservableFieldComponentType.None,
                "Нормальна сила",
                "{0:0.000}",
                "Н"
            );

            _staticFrictionMax = new ObservableFieldComponent<float>(
                _staticFrictionCoefficient.Value * _normalForce.Value,
                ObservableFieldComponentType.None,
                "Макс. тертя спокою",
                "{0:0.000}",
                "Н"
            );

            _kineticFrictionForce = new ObservableFieldComponent<float>(
                _kineticFrictionCoefficient.Value * _normalForce.Value,
                ObservableFieldComponentType.None,
                "Сила тертя ковзання",
                "{0:0.000}",
                "Н"
            );

            _appliedForce = new ObservableFieldComponent<float>(
                0f,
                ObservableFieldComponentType.InputField,
                "Прикладена сила",
                "{0:0.000}",
                "Н"
            );
            _currentAppliedForce = new ObservableFieldComponent<float>(
                0f,
                ObservableFieldComponentType.None,
                "Сила",
                "{0:0.000}",
                "Н"
            );

            _currentStaticFriction = new ObservableFieldComponent<float>(
                0f,
                ObservableFieldComponentType.None,
                "Тертя спокою",
                "{0:0.000}",
                "Н"
            );
        }

        private void UpdateMaterialTexts()
        {
            _boxMaterialText.text = _boxMaterial.Value.DisplayName;
            _groundMaterialText.text = _groundMaterial.Value.DisplayName;
        }

        private void AddAllComponentsToObservables()
        {
            AddObservableFieldComponent(_appliedForce);
            AddObservableFieldComponent(_boxVelocity);
            AddObservableFieldComponent(_staticFrictionMax);
            AddObservableFieldComponent(_kineticFrictionForce);
            AddObservableFieldComponent(_boxMaterial);
            AddObservableFieldComponent(_groundMaterial);
            AddObservableFieldComponent(_staticFrictionCoefficient);
            AddObservableFieldComponent(_kineticFrictionCoefficient);
            AddObservableFieldComponent(_boxWeight);
            AddObservableFieldComponent(_gravity);
            AddObservableFieldComponent(_normalForce);
        }

        private void UpdatePhysics()
        {
            _appliedForceArrow.SetMaxSize(_appliedForce.Value);
            _staticFrictionArrow.SetMaxSize(_staticFrictionMax.Value);

            _currentAppliedForce.TrySetValue(
                Mathf.MoveTowards(_currentAppliedForce.Value, _appliedForce.Value, Time.deltaTime * 75f)
            );
            _currentStaticFriction.TrySetValue(Mathf.Clamp(_currentAppliedForce.Value, 0, _staticFrictionMax.Value));
            if (_currentAppliedForce.Value >= _staticFrictionMax.Value)
            {
                var force = (_currentAppliedForce.Value - _staticFrictionMax.Value) / _boxWeight.Value;
                _currentStaticFriction.TrySetValue(0);
                _staticFrictionArrow.Initialize(_kineticFrictionForce);
                if (_boxRigidbody)
                {
                    _boxRigidbody.AddForce(force * Vector2.right, ForceMode2D.Force);
                }

                _boxVelocity.TrySetValue(_boxRigidbody.linearVelocity.magnitude);
                Debug.Log($"Box velocity: {_boxRigidbody.linearVelocity} m/s.  Speed {force}");
                _staticFrictionArrow.SetForceText($"тертя ковзання");
            }
        }
    }
}
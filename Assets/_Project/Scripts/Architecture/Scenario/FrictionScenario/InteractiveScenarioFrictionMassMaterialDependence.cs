using _Project.Scripts.Architecture.Scenario.Core;
using R3;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.FrictionScenario
{
    public class InteractiveScenarioFrictionMassMaterialDependence : InteractiveScenario
    {
        #region Lifecycle Methods

        private void Awake()
        {
            InitializeComponents();
            InitializeMaterialTexts();
            AddAllComponentsToObservables();
        }

        #endregion

        #region SerializeFields

        [SerializeField] private ForceArrowController _velocityArrowController;
        [SerializeField] private ForceArrowController _frictionArrowController;
        [SerializeField] private TextMeshProUGUI _boxMaterialText;
        [SerializeField] private TextMeshProUGUI _groundMaterialText;

        [SerializeField] private Rigidbody2D _boxRigidbody2D;
        [SerializeField] private Transform _cubeTransform;
        [SerializeField] private Transform _winchTransform;
        [SerializeField] private ProportionalVariableManager _proportionalVariableManager;

        #endregion

        #region Observable Fields

        // Forces
        private ObservableFieldComponent<float> _appliedForce;
        private ObservableFieldComponent<float> _boxVelocity;
        private ObservableFieldComponent<float> _currentAppliedForce;
        private ObservableFieldComponent<float> _currentStaticFriction;
        private ObservableFieldComponent<float> _staticFrictionMax;
        private ObservableFieldComponent<float> _kineticFrictionForce;

        // Materials
        private ObservableFieldComponent<FrictionMaterial> _boxMaterial;
        private ObservableFieldComponent<FrictionMaterial> _groundMaterial;
        private ObservableFieldComponent<float> _staticFrictionCoefficient;
        private ObservableFieldComponent<float> _kineticFrictionCoefficient;

        // Physics
        private ObservableFieldComponent<float> _boxWeight;
        private ObservableFieldComponent<float> _cubeWeight;
        private ObservableFieldComponent<float> _winchEfficiency;
        private ObservableFieldComponent<float> _gravity;
        private ObservableFieldComponent<float> _normalForce;

        #endregion

        #region Private Fields

        private FrictionMaterial _lastBoxMaterial;
        private FrictionMaterial _lastGroundMaterial;

        private Vector3 _initialBoxPosition;
        private Quaternion _initialBoxRotation;
        private Vector3 _initialCubePosition;
        private Quaternion _initialCubeRotation;
        private float _initialDistance;
        private CompositeDisposable _disposables = new();

        #endregion

        #region Interactive Methods

        public override void OnInteractPlay()
        {
            Debug.Log($"Starting simulation");
            ResetPhysicsStateImmediate();

            Observable.EveryUpdate(UnityFrameProvider.FixedUpdate)
                .Subscribe(_ => UpdatePhysics())
                .AddTo(_disposables);
        }

        public override void OnInteractStop()
        {
            Debug.Log($"Stopping simulation");
            _disposables.Clear();

            ResetPhysicsStateImmediate();
        }

        private void UpdatePhysics()
        {
            _velocityArrowController.SetMaxSize(_appliedForce.Value);
            _frictionArrowController.SetMaxSize(_staticFrictionMax.Value);

            _currentAppliedForce.TrySetValue(_appliedForce.Value);
            _currentStaticFriction.TrySetValue(Mathf.Clamp(_currentAppliedForce.Value, 0, _staticFrictionMax.Value));

            if (_currentAppliedForce.Value >= _staticFrictionMax.Value)
            {
                var netForce = _currentAppliedForce.Value - _kineticFrictionForce.Value;
                var acceleration = netForce / _boxWeight.Value;

                _currentStaticFriction.TrySetValue(0);
                _frictionArrowController.Initialize(_kineticFrictionForce);

                if (_boxRigidbody2D)
                {
                    if (netForce > 0)
                    {
                        var maxVelocity =
                            netForce / _kineticFrictionForce.Value * 5f;

                        if (_boxRigidbody2D.linearVelocity.magnitude < maxVelocity)
                        {
                            _boxRigidbody2D.AddForce(acceleration * Vector2.right, ForceMode2D.Force);
                        }
                        else
                        {
                            _boxRigidbody2D.linearVelocity = _boxRigidbody2D.linearVelocity.normalized * maxVelocity;
                        }
                    }
                }

                _boxVelocity.TrySetValue(_boxRigidbody2D.linearVelocity.magnitude);
            }

            var distance = Vector2.Distance(_boxRigidbody2D.transform.position, _winchTransform.position);
            distance = _initialDistance - distance;
            _proportionalVariableManager.SetVariableA(distance);

            var offset = new Vector3(0, _proportionalVariableManager.GetVariableB(), 0);
            _cubeTransform.transform.position = _initialCubePosition - offset;
        }

        #endregion

        #region Initialization Methods

        private void InitializeComponents()
        {
            InitializeMaterials();
            InitializeFrictionCoefficients();
            InitializePhysicsComponents();
            InitializeForcesComponents();
            InitializeRestrictions();
            InitializeEvents();
            InitializeArrows();
            InitializeInitialState();
        }

        private void InitializeMaterials()
        {
            var allMaterials = FrictionMaterialRegistry.Instance.GetAllMaterials();

            _boxMaterial = new ObservableFieldComponent<FrictionMaterial>(
                FrictionMaterialRegistry.Instance.GetById("wood"),
                ObservableFieldComponentType.Dropdown,
                "Матеріал коробки"
            );
            _boxMaterial.SetAvailableOptions(allMaterials);

            _groundMaterial = new ObservableFieldComponent<FrictionMaterial>(
                FrictionMaterialRegistry.Instance.GetById("concrete"),
                ObservableFieldComponentType.Dropdown,
                "Матеріал підлоги"
            );
            _groundMaterial.SetAvailableOptions(allMaterials);
        }

        private void InitializeFrictionCoefficients()
        {
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
        }

        private void InitializePhysicsComponents()
        {
            _boxWeight = new ObservableFieldComponent<float>(
                10f,
                ObservableFieldComponentType.InputField,
                "Вага коробки(кг)",
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

            _cubeWeight = new ObservableFieldComponent<float>(
                10f,
                ObservableFieldComponentType.InputField,
                "Вага куба (кг)",
                "{0:0.#}",
                "кг"
            );

            _normalForce = new ObservableFieldComponent<float>(
                _boxWeight.Value * _gravity.Value,
                ObservableFieldComponentType.None,
                "Нормальна сила",
                "{0:0.000}",
                "Н"
            );

            _winchEfficiency = new ObservableFieldComponent<float>(
                0.85f,
                ObservableFieldComponentType.None,
                "Коефіцієнт корисної дії лебідки",
                "{0:0.00}"
            );
        }

        private void InitializeForcesComponents()
        {
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
            Debug.Log(
                $"Куб: {_cubeWeight.Value} * Гравітація: {_gravity.Value} * КПД лебідки: {_winchEfficiency.Value} = {(_cubeWeight.Value * _gravity.Value) * _winchEfficiency.Value}"
            );
            var force = _cubeWeight.Value * _gravity.Value * _winchEfficiency.Value;
            _appliedForce = new ObservableFieldComponent<float>(
                force,
                ObservableFieldComponentType.None,
                "Сила натягу",
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

            _boxVelocity = new ObservableFieldComponent<float>(
                0f,
                ObservableFieldComponentType.None,
                "Швидкість коробки",
                "{0:0.00}",
                "м/с"
            );
        }

        private void InitializeRestrictions()
        {
            var greaterOrEqualRestriction = new GreaterThanOrEqualRestriction<float>(0f);

            _boxWeight.AddRestriction(greaterOrEqualRestriction);
            _cubeWeight.AddRestriction(greaterOrEqualRestriction);
            _gravity.AddRestriction(greaterOrEqualRestriction);
            _winchEfficiency.AddRestriction(new RangeRestriction<float>(0.1f, 1.0f));
        }

        private void InitializeEvents()
        {
            _boxMaterial.ValueObservable.DistinctUntilChanged().Subscribe(material =>
                {
                    OnMaterialChanged();
                    _boxMaterialText.text = material.DisplayName;
                }
            ).AddTo(this);

            _groundMaterial.ValueObservable.DistinctUntilChanged().Subscribe(material =>
                {
                    OnMaterialChanged();
                    _groundMaterialText.text = material.DisplayName;
                }
            ).AddTo(this);

            _boxWeight.ValueObservable
                .DistinctUntilChanged()
                .Subscribe(x =>
                    {
                        _normalForce.TrySetValue(x * _gravity.Value);
                        UpdateFrictionForces();
                    }
                )
                .AddTo(this);

            _cubeWeight.ValueObservable
                .CombineLatest(_winchEfficiency.ValueObservable, (weight, efficiency) => weight * efficiency)
                .DistinctUntilChanged()
                .Subscribe(effectiveWeight => _appliedForce.TrySetValue(effectiveWeight * _gravity.Value))
                .AddTo(this);

            _gravity.ValueObservable
                .DistinctUntilChanged()
                .Subscribe(_ =>
                    {
                        _normalForce.TrySetValue(_boxWeight.Value * _gravity.Value);
                        _appliedForce.TrySetValue(_cubeWeight.Value * _gravity.Value * _winchEfficiency.Value);
                        UpdateFrictionForces();
                    }
                )
                .AddTo(this);
        }

        private void InitializeInitialState()
        {
            _initialBoxPosition = _boxRigidbody2D.transform.position;
            _initialBoxRotation = _boxRigidbody2D.transform.rotation;
            _initialCubePosition = _cubeTransform.position;
            _initialCubeRotation = _cubeTransform.rotation;
            _initialDistance = Vector2.Distance(_boxRigidbody2D.transform.position, _winchTransform.position);
        }

        private void InitializeMaterialTexts()
        {
            _boxMaterialText.text = _boxMaterial.Value.DisplayName;
            _groundMaterialText.text = _groundMaterial.Value.DisplayName;
        }

        private void InitializeArrows()
        {
            _velocityArrowController.Initialize(_boxVelocity);
            _frictionArrowController.Initialize(_currentStaticFriction);
        }

        #endregion

        #region Event Handlers

        private void OnMaterialChanged()
        {
            if (_boxMaterial.Value == _lastBoxMaterial && _groundMaterial.Value == _lastGroundMaterial)
                return;

            _lastBoxMaterial = _boxMaterial.Value;
            _lastGroundMaterial = _groundMaterial.Value;

            var staticCoefficient = FrictionBaseData.GetStaticCoefficient(_boxMaterial.Value, _groundMaterial.Value);
            var kineticCoefficient = FrictionBaseData.GetKineticCoefficient(_boxMaterial.Value, _groundMaterial.Value);

            _staticFrictionCoefficient.TrySetValue(staticCoefficient);
            _kineticFrictionCoefficient.TrySetValue(kineticCoefficient);

            UpdateFrictionForces();
        }

        private void UpdateFrictionForces()
        {
            float normalForceValue = _normalForce.Value;
            _staticFrictionMax.TrySetValue(_staticFrictionCoefficient.Value * normalForceValue);
            _kineticFrictionForce.TrySetValue(_kineticFrictionCoefficient.Value * normalForceValue);
        }

        #endregion

        #region Utility Methods

        private void ResetPhysicsStateImmediate()
        {
            _boxRigidbody2D.transform.position = _initialBoxPosition;
            _boxRigidbody2D.transform.rotation = _initialBoxRotation;
            _cubeTransform.position = _initialCubePosition;
            _cubeTransform.rotation = _initialCubeRotation;

            _boxRigidbody2D.linearVelocity = Vector2.zero;
            _boxRigidbody2D.angularVelocity = 0f;
            _boxVelocity.TrySetValue(0f);
            _currentAppliedForce.TrySetValue(0f);
            _currentStaticFriction.TrySetValue(0f);
            _frictionArrowController.SetForceText($"тертя сп");
            _proportionalVariableManager.SetVariableA(0f);
        }

        private void AddAllComponentsToObservables()
        {
            AddObservableFieldComponent(_boxVelocity);

            AddObservableFieldComponent(_boxMaterial);
            AddObservableFieldComponent(_groundMaterial);
            AddObservableFieldComponent(_boxWeight);
            AddObservableFieldComponent(_cubeWeight);
            AddObservableFieldComponent(_gravity);
            AddObservableFieldComponent(_winchEfficiency);

            AddObservableFieldComponent(_staticFrictionCoefficient);
            AddObservableFieldComponent(_kineticFrictionCoefficient);

            AddObservableFieldComponent(_normalForce);
            AddObservableFieldComponent(_staticFrictionMax);
            AddObservableFieldComponent(_kineticFrictionForce);
            AddObservableFieldComponent(_appliedForce);
        }

        #endregion
    }
}
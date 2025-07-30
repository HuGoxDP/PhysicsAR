using System;
using System.Collections.Generic;
using _Project.Scripts.Architecture.Scenario.Core;
using _Project.Scripts.Architecture.Scenario.FrictionScenario.Util;
using R3;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.FrictionScenario
{
    [Serializable]
    public class FrictionItem
    {
        public string Name;
        public TextMeshProUGUI Text;
        public GameObject ItemGameObject;

        public FrictionItem()
        {
            Text = null;
            ItemGameObject = null;
        }

        public FrictionItem(TextMeshProUGUI text, GameObject texture)
        {
            Text = text;
            ItemGameObject = texture;
        }
    }

    [Serializable]
    public class BoxAreaFrictionAreaType : FrictionItem
    {
        public float Area;

        public BoxAreaFrictionAreaType()
        {
            Area = 0f;
        }

        public BoxAreaFrictionAreaType(TextMeshProUGUI text, GameObject texture, float area) : base(text, texture)
        {
            Area = area;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }

    public class InteractiveScenarioContactAreaFriction : InteractiveScenario
    {
        #region Serialized Fields

        [SerializeField] private TextMeshProUGUI _groundMaterialText;

        [SerializeField] private Rigidbody2D _boxRigidbody2D;
        [SerializeField] private DynamometerController _dynamometer;

        [SerializeField] private List<BoxAreaFrictionAreaType> _areaTypeList;

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

        private ObservableFieldComponent<BoxAreaFrictionAreaType> _areaType;
        private ObservableFieldComponent<float> _area;

        // Physics
        private ObservableFieldComponent<float> _boxWeight;
        private ObservableFieldComponent<float> _gravity;
        private ObservableFieldComponent<float> _normalForce;

        #endregion

        #region private fields

        private FrictionMaterial _lastBoxMaterial;
        private FrictionMaterial _lastGroundMaterial;

        private Vector3 _initialBoxPosition;
        private Quaternion _initialBoxRotation;

        private Vector3 _initialDynamometerPosition;
        private Quaternion _initialDynamometerRotation;

        private CompositeDisposable _disposables = new();

        #endregion

        #region Lifecycle Methods

        private void Awake()
        {
            InitializeComponents();
            InitializeMaterialTexts();
            AddAllComponentsToObservables();
        }

        private void Start()
        {
            _dynamometer.SetBodyAnimation();
            _dynamometer.SetValue(0f);
        }

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
            float newDynamometerValue = CalculateDynamometerValue(
                _currentAppliedForce.Value,
                _staticFrictionMax.Value,
                _kineticFrictionForce.Value
            );
            float currentDynamometerValue = _dynamometer.GetValue();
            float dynamometerValue = Mathf.MoveTowards(
                currentDynamometerValue,
                newDynamometerValue,
                Time.deltaTime * 10f
            );
            _dynamometer.SetValue(dynamometerValue);

            _currentAppliedForce.TrySetValue(
                Mathf.MoveTowards(_currentAppliedForce.Value, _appliedForce.Value, Time.deltaTime * 1f)
            );
            _currentStaticFriction.TrySetValue(Mathf.Clamp(_currentAppliedForce.Value, 0, _staticFrictionMax.Value));

            if (_currentAppliedForce.Value >= _staticFrictionMax.Value)
            {
                var netForce = _currentAppliedForce.Value - _kineticFrictionForce.Value;
                var acceleration = netForce / _boxWeight.Value;

                _currentStaticFriction.TrySetValue(0);

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

                float boxVelocityX = _boxRigidbody2D.linearVelocity.x;
                _dynamometer.transform.position += new Vector3(boxVelocityX, 0, 0) * Time.deltaTime;

                _boxVelocity.TrySetValue(_boxRigidbody2D.linearVelocity.magnitude);
            }
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
            InitializeInitialState();
        }

        private void InitializeMaterials()
        {
            var allMaterials = FrictionMaterialRegistry.Instance.GetAllMaterials();

            _boxMaterial = new ObservableFieldComponent<FrictionMaterial>(
                FrictionMaterialRegistry.Instance.GetById("wood"),
                ObservableFieldComponentType.None,
                "Матеріал коробки"
            );
            _boxMaterial.SetAvailableOptions(allMaterials);

            _groundMaterial = new ObservableFieldComponent<FrictionMaterial>(
                FrictionMaterialRegistry.Instance.GetById("concrete"),
                ObservableFieldComponentType.None,
                "Матеріал підлоги"
            );
            _groundMaterial.SetAvailableOptions(allMaterials);


            _areaType = new ObservableFieldComponent<BoxAreaFrictionAreaType>(
                _areaTypeList[0],
                ObservableFieldComponentType.Dropdown,
                "Площа контакту"
            );
            _areaType.SetAvailableOptions(_areaTypeList);

            _area = new ObservableFieldComponent<float>(
                _areaType.Value.Area,
                ObservableFieldComponentType.None,
                "Площа контакту",
                "{0:0.00}",
                "м²"
            );
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
                1f,
                ObservableFieldComponentType.None,
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

            _normalForce = new ObservableFieldComponent<float>(
                _boxWeight.Value * _gravity.Value,
                ObservableFieldComponentType.None,
                "Нормальна сила",
                "{0:0.000}",
                "Н"
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

            _appliedForce = new ObservableFieldComponent<float>(
                10f,
                ObservableFieldComponentType.None,
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
            _gravity.AddRestriction(greaterOrEqualRestriction);
        }

        private void InitializeEvents()
        {
            _areaType.ValueObservable.DistinctUntilChanged().Subscribe(areaType =>
                {
                    OnMaterialChanged();
                    foreach (var t in _areaTypeList)
                    {
                        t.ItemGameObject.SetActive(t == areaType);
                    }

                    _area.TrySetValue(areaType.Area);
                }
            ).AddTo(this);

            _boxMaterial.ValueObservable.DistinctUntilChanged().Subscribe(material =>
                {
                    OnMaterialChanged();
                    foreach (var t in _areaTypeList)
                    {
                        t.Text.text = material.DisplayName;
                    }
                }
            ).AddTo(this);

            _groundMaterial.ValueObservable.DistinctUntilChanged().Subscribe(material =>
                {
                    OnMaterialChanged();
                    _groundMaterialText.text = material.DisplayName;
                }
            ).AddTo(this);
        }

        private void InitializeInitialState()
        {
            _initialBoxPosition = _boxRigidbody2D.transform.position;
            _initialBoxRotation = _boxRigidbody2D.transform.rotation;
            _initialDynamometerPosition = _dynamometer.transform.position;
            _initialDynamometerRotation = _dynamometer.transform.rotation;
        }

        private void InitializeMaterialTexts()
        {
            foreach (var t in _areaTypeList)
            {
                t.Text.text = _boxMaterial.Value.DisplayName;
            }

            _groundMaterialText.text = _groundMaterial.Value.DisplayName;
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
            _dynamometer.transform.position = _initialDynamometerPosition;
            _dynamometer.transform.rotation = _initialDynamometerRotation;
            _dynamometer.SetValue(0f);

            _boxRigidbody2D.linearVelocity = Vector2.zero;
            _boxRigidbody2D.angularVelocity = 0f;
            _boxVelocity.TrySetValue(0f);
            _currentAppliedForce.TrySetValue(0f);
            _currentStaticFriction.TrySetValue(0f);
        }

        private void AddAllComponentsToObservables()
        {
            AddObservableFieldComponent(_boxVelocity);
            AddObservableFieldComponent(_areaType);
            AddObservableFieldComponent(_area);

            AddObservableFieldComponent(_boxMaterial);
            AddObservableFieldComponent(_groundMaterial);
            AddObservableFieldComponent(_boxWeight);
            AddObservableFieldComponent(_gravity);

            AddObservableFieldComponent(_staticFrictionCoefficient);
            AddObservableFieldComponent(_kineticFrictionCoefficient);

            AddObservableFieldComponent(_normalForce);
            AddObservableFieldComponent(_staticFrictionMax);
            AddObservableFieldComponent(_kineticFrictionForce);
            AddObservableFieldComponent(_appliedForce);
        }


        private float
            CalculateDynamometerValue(float appliedForce, float staticFrictionMax, float kineticFrictionMax) =>
            appliedForce <= staticFrictionMax ? appliedForce : kineticFrictionMax;

        #endregion
    }
}
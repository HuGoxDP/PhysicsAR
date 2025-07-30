using _Project.Scripts.Architecture.Core;
using _Project.Scripts.Architecture.ScenarioDataTransfer;
using UnityEngine;
using Vuforia;
using Zenject;

namespace _Project.Scripts.Architecture
{
    public class TargetController : VuforiaMonoBehaviour
    {
        [SerializeField] private ImageTargetData _imageTargetData;

        [Inject] private ScenarioData _scenarioData;
        [Inject] private SceneLoader _sceneLoader;
        private ISpawnButton _spawnButton;
        private DefaultObserverEventHandler _trackableBehaviour;

        private void Awake()
        {
            _trackableBehaviour = GetComponent<DefaultObserverEventHandler>();
            _spawnButton = GetComponentInChildren<SpawnButton>();

            if (_spawnButton == null)
            {
                Debug.LogError($"SpawnButton не найден на объекте {gameObject.name}");
            }
        }

        private void Start()
        {
            if (_trackableBehaviour)
            {
                _trackableBehaviour.OnTargetFound.AddListener(OnTargetFound);
                _trackableBehaviour.OnTargetLost.AddListener(OnTargetLost);
            }

            if (_spawnButton != null)
            {
                _spawnButton.OnButtonClicked += SpawnObject;
                _spawnButton.Disable();
            }
        }

        private void OnDestroy()
        {
            if (_trackableBehaviour)
            {
                _trackableBehaviour.OnTargetFound.RemoveListener(OnTargetFound);
                _trackableBehaviour.OnTargetLost.RemoveListener(OnTargetLost);
            }

            if (_spawnButton != null)
            {
                _spawnButton.OnButtonClicked -= SpawnObject;
            }
        }

        private void OnTargetFound()
        {
            if (_imageTargetData.AutoLoad)
            {
                SpawnObject();
            }
            else
            {
                _spawnButton?.Enable();
            }
        }

        private void OnTargetLost()
        {
            _spawnButton?.Disable();
        }

        private void SpawnObject()
        {
            if (_imageTargetData?.SpawnObject != null)
            {
                _scenarioData.ImageTargetData = _imageTargetData;
                _sceneLoader.LoadScene(1);
            }
        }
    }
}
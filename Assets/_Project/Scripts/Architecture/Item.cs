using _Project.Scripts.Architecture.Core;
using _Project.Scripts.Architecture.ScenarioDataTransfer;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Architecture
{
    public class Item : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] [ReadOnly] private ImageTargetData _imageTargetData;
        [field: SerializeField] public Vector2 Size { get; private set; }
        private Button _button;

        private ScenarioData _scenarioData;
        private SceneLoader _sceneLoader;


        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        public Item Init(ImageTargetData imageTargetData, ScenarioData scenarioData, SceneLoader sceneLoader)
        {
            _scenarioData = scenarioData;
            _sceneLoader = sceneLoader;
            _imageTargetData = imageTargetData;
            _nameText.text = imageTargetData.Name;
            return this;
        }

        private void OnButtonClicked()
        {
            if (_scenarioData == null)
            {
                Debug.LogError("ScenarioData is not assigned!", this);
                return;
            }

            if (_sceneLoader == null)
            {
                Debug.LogError("SceneLoader is not assigned!", this);
                return;
            }

            _scenarioData.ImageTargetData = _imageTargetData;
            _sceneLoader.LoadScene(1);
        }
    }
}
using System;
using _Project.Scripts.Architecture.Scenario.Core;
using _Project.Scripts.Architecture.ScenarioDataTransfer;
using _Project.Scripts.Architecture.UI.Interfaces;
using Zenject;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Architecture
{
    public class ScenarioInitializer : IInitializable
    {
        private readonly ScenarioData _scenarioData;
        private readonly IUIManager _uiManager;

        public ScenarioInitializer(ScenarioData scenarioData, IUIManager uiManager)
        {
            _scenarioData = scenarioData;
            _uiManager = uiManager;
        }

        public void Initialize()
        {
            // Initialize the scenario data
            if (_scenarioData == null)
            {
                throw new ArgumentNullException(nameof(_scenarioData), "ScenarioData is not assigned in inspector!");
            }

            var imageTargetData = _scenarioData.ImageTargetData;
            var obj = Object.Instantiate(imageTargetData.SpawnObject);
            var baseScenario = obj.GetComponent<BaseScenario>();
            baseScenario.SetDisplayableInfo(imageTargetData);
            baseScenario.SetAutoPlay(imageTargetData.AutoLoad);
            _uiManager.SwitchToUI(imageTargetData.TargetUIType);
            if (_uiManager.CurrentUI is IScenarioUI scenarioUI)
            {
                scenarioUI.SetScenario(baseScenario);
            }
        }
    }
}
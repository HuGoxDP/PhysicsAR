using UnityEngine;

namespace _Project.Scripts.Architecture.ScenarioDataTransfer
{
    [CreateAssetMenu(fileName = "SceneData", menuName = "Scenarios/SceneData")]
    public class ScenarioData : RuntimeScriptableObject
    {
        public ImageTargetData ImageTargetData;

        protected override void OnReset()
        {
            ImageTargetData = null;
        }
    }
}
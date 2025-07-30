using _Project.Scripts.Architecture.UI.Core;
using UnityEngine;

namespace _Project.Scripts.Architecture
{
    [CreateAssetMenu(fileName = "ImageTargetData", menuName = "Target/ImageTargetData", order = 0)]
    public class ImageTargetData : ScriptableObject, IDisplayableInfo
    {
        [field: SerializeField] public GameObject SpawnObject { get; private set; }
        [field: SerializeField] public UIType TargetUIType { get; private set; }
        [field: SerializeField] public bool AutoLoad { get; private set; }
        [field: SerializeField] public string Name { get; private set; } = "Name";

        [field: SerializeField, TextArea(4, 40)]
        public string Description { get; private set; } = "Description";
    }
}
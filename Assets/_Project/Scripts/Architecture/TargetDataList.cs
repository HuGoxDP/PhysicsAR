using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Architecture
{
    [CreateAssetMenu(fileName = "TargetDataList", menuName = "Target/TargetDataList", order = 1)]
    public class TargetDataList : ScriptableObject
    {
        public List<ImageTargetData> DataList;
    }
}
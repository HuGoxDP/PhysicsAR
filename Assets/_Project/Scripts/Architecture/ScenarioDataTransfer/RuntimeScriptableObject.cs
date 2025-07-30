using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Architecture.ScenarioDataTransfer
{
    public abstract class RuntimeScriptableObject : ScriptableObject
    {
        static readonly List<RuntimeScriptableObject> Instances = new List<RuntimeScriptableObject>();

        void OnEnable() => Instances.Add(this);
        void OnDisable() => Instances.Remove(this);

        protected abstract void OnReset();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetAllInstances()
        {
            foreach (var instance in Instances)
            {
                instance.OnReset();
            }
        }
    }
}
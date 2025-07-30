#if !NOT_UNITY3D

using ModestTree;
using UnityEngine;

namespace Zenject
{
    [NoReflectionBaking]
    public class PrefabProvider : IPrefabProvider
    {
        readonly Object _prefab;

        public PrefabProvider(Object prefab)
        {
            Assert.IsNotNull(prefab);
            _prefab = prefab;
        }

        public Object GetPrefab(InjectContext _)
        {
            return _prefab;
        }
    }
}

#endif
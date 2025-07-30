#if !NOT_UNITY3D

using ModestTree;
using UnityEngine;

namespace Zenject
{
    [NoReflectionBaking]
    public class PrefabProviderCustom : IPrefabProvider
    {
        readonly System.Func<InjectContext, Object> _getter;

        public PrefabProviderCustom(System.Func<InjectContext, Object> getter)
        {
            _getter = getter;
        }

        public Object GetPrefab(InjectContext context)
        {
            var prefab = _getter(context);
            Assert.That(prefab != null, "Custom prefab provider returned null");
            return prefab;
        }
    }
}

#endif
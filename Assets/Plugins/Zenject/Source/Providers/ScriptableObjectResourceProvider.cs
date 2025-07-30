#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;
using Zenject.Internal;

namespace Zenject
{
    [NoReflectionBaking]
    public class ScriptableObjectResourceProvider : IProvider
    {
        readonly object _concreteIdentifier;
        readonly DiContainer _container;
        readonly bool _createNew;
        readonly List<TypeValuePair> _extraArguments;
        readonly Action<InjectContext, object> _instantiateCallback;
        readonly string _resourcePath;
        readonly Type _resourceType;

        public ScriptableObjectResourceProvider(
            string resourcePath, Type resourceType,
            DiContainer container, IEnumerable<TypeValuePair> extraArguments,
            bool createNew, object concreteIdentifier,
            Action<InjectContext, object> instantiateCallback)
        {
            _container = container;
            Assert.DerivesFromOrEqual<ScriptableObject>(resourceType);

            _extraArguments = extraArguments.ToList();
            _resourceType = resourceType;
            _resourcePath = resourcePath;
            _createNew = createNew;
            _concreteIdentifier = concreteIdentifier;
            _instantiateCallback = instantiateCallback;
        }

        public bool IsCached
        {
            get { return false; }
        }

        public bool TypeVariesBasedOnMemberType
        {
            get { return false; }
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _resourceType;
        }

        public void GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction, List<object> buffer)
        {
            Assert.IsNotNull(context);

            if (_createNew)
            {
                var objects = Resources.LoadAll(_resourcePath, _resourceType);

                for (int i = 0; i < objects.Length; i++)
                {
                    buffer.Add(ScriptableObject.Instantiate(objects[i]));
                }
            }
            else
            {
                buffer.AllocFreeAddRange(
                    Resources.LoadAll(_resourcePath, _resourceType)
                );
            }

            Assert.That(
                buffer.Count > 0,
                "Could not find resource at path '{0}' with type '{1}'",
                _resourcePath,
                _resourceType
            );

            injectAction = () =>
            {
                for (int i = 0; i < buffer.Count; i++)
                {
                    var obj = buffer[i];

                    var extraArgs = ZenPools.SpawnList<TypeValuePair>();

                    extraArgs.AllocFreeAddRange(_extraArguments);
                    extraArgs.AllocFreeAddRange(args);

                    _container.InjectExplicit(
                        obj,
                        _resourceType,
                        extraArgs,
                        context,
                        _concreteIdentifier
                    );

                    ZenPools.DespawnList(extraArgs);

                    if (_instantiateCallback != null)
                    {
                        _instantiateCallback(context, obj);
                    }
                }
            };
        }
    }
}

#endif
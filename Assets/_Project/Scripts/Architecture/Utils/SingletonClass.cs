using System;
using HuGox.Utils;
using UnityEngine;

namespace _Project.Scripts.Architecture.Utils
{
    public abstract class SingletonClass<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        protected static readonly object InstanceLock = new();
        private static bool _isApplicationQuitting;

        public static T Instance
        {
            get
            {
                if (_isApplicationQuitting)
                {
                    throw new Exception(
                        $"Instance of {typeof(T)} already destroyed on application quit. Returning null."
                    );
                }

                lock (InstanceLock)
                {
                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject(typeof(T).Name);
                        _instance = singleton.AddComponent<T>();
                        DontDestroyOnLoad(singleton);
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                throw new Exception($"Instance of {typeof(T)} already exists. Destroying duplicate.");
            }

            _instance = gameObject.GetComponentOrAdd<T>();
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }
    }
}
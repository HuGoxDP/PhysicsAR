using System;
using System.Collections.Generic;
using UnityEngine;

namespace HuGox.Utils
{
    [Serializable]
    public class Vector3Serializable
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public static implicit operator Vector3Serializable(Vector3 vector3)
        {
            var vector3Serializable = new Vector3Serializable
            {
                X = vector3.x,
                Y = vector3.y,
                Z = vector3.z
            };
            return vector3Serializable;
        }

        public static implicit operator Vector3(Vector3Serializable vector3Serializable)
        {
            var vector3 = new Vector3
            {
                x = vector3Serializable.X,
                y = vector3Serializable.Y,
                z = vector3Serializable.Z
            };
            return vector3;
        }
    }

    [Serializable]
    public class SerializedArray<T>
    {
        [SerializeField] private List<T> _array = new List<T>();
        public T this[int index] => _array[index];

        public int Count => _array.Count;

        public List<T> GetArray()
        {
            return _array;
        }

        public void Add(T item)
        {
            _array.Add(item);
        }

        public void RemoveAt(int itemId)
        {
            _array.RemoveAt(itemId);
        }

        public void Remove(T item)
        {
            _array.Remove(item);
        }
    }

    [Serializable]
    public class SerializableDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<K> _mKeys = new List<K>();
        [SerializeField] private List<V> _mValues = new List<V>();

        public void OnBeforeSerialize()
        {
            _mKeys.Clear();
            _mValues.Clear();
            foreach (KeyValuePair<K, V> pair in this)
            {
                _mKeys.Add(pair.Key);
                _mValues.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            if (_mKeys.Count != _mValues.Count)
            {
                Debug.LogError(
                    $"Deserialization failed: keys count ({_mKeys.Count}) does not match values count ({_mValues.Count})."
                );
                return;
            }

            for (int i = 0; i < _mKeys.Count; i++)
            {
                if (_mKeys[i] == null || _mKeys[i].Equals(default(K)))
                {
                    Debug.LogWarning("Skipping empty or default key during deserialization.");
                    continue;
                }

                if (!ContainsKey(_mKeys[i]))
                {
                    Add(_mKeys[i], _mValues[i]);
                }
                else
                {
                    Debug.LogWarning($"Duplicate key found during deserialization: {_mKeys[i]}. Skipping this entry.");
                }
            }
        }
    }
}
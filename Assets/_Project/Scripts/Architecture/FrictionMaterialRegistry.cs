using System;
using System.Collections.Generic;

namespace _Project.Scripts.Architecture
{
    public class FrictionMaterialRegistry
    {
        private static readonly Lazy<FrictionMaterialRegistry> _instance = new(() => new FrictionMaterialRegistry());
        private readonly IReadOnlyCollection<FrictionMaterial> _cachedMaterialsCollection;

        private readonly List<FrictionMaterial> _cachedMaterialsList;

        private readonly Dictionary<string, FrictionMaterial> _materials = new(StringComparer.OrdinalIgnoreCase);

        private FrictionMaterialRegistry()
        {
            _cachedMaterialsList = new List<FrictionMaterial>();
            _cachedMaterialsCollection = _cachedMaterialsList.AsReadOnly();
        }

        public static FrictionMaterialRegistry Instance => _instance.Value;

        internal void Register(FrictionMaterial material)
        {
            if (_materials.ContainsKey(material.Id))
                throw new ArgumentException("Material already registered.", nameof(material));

            _materials[material.Id] = material;
            _cachedMaterialsList.Add(material);
        }


        public FrictionMaterial GetById(string id)
        {
            return _materials.TryGetValue(id, out var material)
                ? material
                : throw new KeyNotFoundException($"Material with ID '{id}' not found.");
        }

        public IReadOnlyCollection<FrictionMaterial> GetAllMaterials()
        {
            return _cachedMaterialsCollection;
        }
    }
}
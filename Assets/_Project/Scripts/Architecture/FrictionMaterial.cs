using System;

namespace _Project.Scripts.Architecture
{
    public sealed class FrictionMaterial
    {
        private FrictionMaterial(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public string Id { get; }
        public string DisplayName { get; }

        public static FrictionMaterial Register(string id, string displayName)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));

            var material = new FrictionMaterial(id, displayName);
            FrictionMaterialRegistry.Instance.Register(material);
            return material;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
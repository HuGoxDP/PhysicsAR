using System;
using System.Collections.Generic;

namespace _Project.Scripts.Architecture
{
    public static class FrictionBaseData
    {
        public const float Gravity = 9.81f; // m/s^2
        private static readonly Dictionary<(FrictionMaterial, FrictionMaterial), float> StaticCoefficients = new();
        private static readonly Dictionary<(FrictionMaterial, FrictionMaterial), float> KineticCoefficients = new();

        public static void AddStaticCoefficient(FrictionMaterial m1, FrictionMaterial m2, float coefficient)
        {
            var key = OrderedPair(m1, m2);
            StaticCoefficients[key] = coefficient;
        }

        public static void AddKineticCoefficient(FrictionMaterial m1, FrictionMaterial m2, float coefficient)
        {
            var key = OrderedPair(m1, m2);
            KineticCoefficients[key] = coefficient;
        }

        public static float GetStaticCoefficient(FrictionMaterial m1, FrictionMaterial m2)
        {
            var key = OrderedPair(m1, m2);
            return StaticCoefficients.TryGetValue(key, out var coefficient)
                ? coefficient
                : throw new KeyNotFoundException($"Static coefficient for {m1.Id}-{m2.Id} not found");
        }

        public static float GetKineticCoefficient(FrictionMaterial m1, FrictionMaterial m2)
        {
            var key = OrderedPair(m1, m2);
            return KineticCoefficients.TryGetValue(key, out var coefficient)
                ? coefficient
                : throw new KeyNotFoundException($"Kinetic coefficient for {m1.Id}-{m2.Id} not found");
        }

        private static (FrictionMaterial, FrictionMaterial) OrderedPair(FrictionMaterial a, FrictionMaterial b)
        {
            return StringComparer.Ordinal.Compare(a.Id, b.Id) < 0 ? (a, b) : (b, a);
        }
    }
}
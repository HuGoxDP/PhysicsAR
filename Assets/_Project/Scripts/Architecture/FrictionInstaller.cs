using UnityEngine;

namespace _Project.Scripts.Architecture
{
    public class FrictionInstaller : MonoBehaviour
    {
        private FrictionInstaller Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }


            // Реєстрація матеріалів
            FrictionMaterial.Register("concrete", "Бетон");
            FrictionMaterial.Register("wood", "Дерево");
            FrictionMaterial.Register("steel", "Сталь");
            FrictionMaterial.Register("rubber", "Гума");
            FrictionMaterial.Register("plastic", "Пластик");

            // Реєстрація коефіцієнтів (статичні/кінетичні)
            RegisterFrictionPairs();
        }

        private void RegisterFrictionPairs()
        {
            // Дерево
            RegisterPair("wood", "wood", 0.38f, 0.25f);
            RegisterPair("wood", "concrete", 0.55f, 0.45f);
            RegisterPair("wood", "steel", 0.4f, 0.2f);
            RegisterPair("wood", "rubber", 0.7f, 0.5f);
            RegisterPair("wood", "plastic", 0.35f, 0.2f);

            // Бетон
            RegisterPair("concrete", "concrete", 0.7f, 0.6f);
            RegisterPair("concrete", "steel", 0.4f, 0.3f);
            RegisterPair("concrete", "rubber", 0.8f, 0.6f);
            RegisterPair("concrete", "plastic", 0.5f, 0.3f);

            // Сталь
            RegisterPair("steel", "steel", 0.65f, 0.4f);
            RegisterPair("steel", "rubber", 0.6f, 0.4f);
            RegisterPair("steel", "plastic", 0.3f, 0.2f);

            // Резина
            RegisterPair("rubber", "rubber", 1.1f, 0.8f);
            RegisterPair("rubber", "plastic", 0.65f, 0.4f);

            // Пластик
            RegisterPair("plastic", "plastic", 0.3f, 0.2f);
        }

        private void RegisterPair(string id1, string id2, float staticCoef, float kineticCoef)
        {
            var m1 = FrictionMaterialRegistry.Instance.GetById(id1);
            var m2 = FrictionMaterialRegistry.Instance.GetById(id2);

            FrictionBaseData.AddStaticCoefficient(m1, m2, staticCoef);
            FrictionBaseData.AddKineticCoefficient(m1, m2, kineticCoef);
        }
    }
}
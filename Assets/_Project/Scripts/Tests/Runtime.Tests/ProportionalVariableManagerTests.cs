using System.Reflection;
using _Project.Scripts.Architecture.Scenario.FrictionScenario;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Scripts.Tests.Runtime.Tests
{
    public class ProportionalVariableManagerTests
    {
        private ProportionalVariableManager _manager;
        private GameObject _managerObject;
        private IProportionalVariable _variableA;
        private IProportionalVariable _variableB;

        [SetUp]
        public void Setup()
        {
            // Создаем объект для менеджера
            _managerObject = new GameObject("Manager");
            _manager = _managerObject.AddComponent<ProportionalVariableManager>();

            // Создаем экземпляры ProportionalVariable
            _variableA = new ProportionalVariable(100f, 0f, 0f);
            _variableB = new ProportionalVariable(50f, 0f, 0f);

            // Устанавливаем переменные в менеджер через рефлексию
            typeof(ProportionalVariableManager)
                .GetField("_variableA", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_manager, _variableA);

            typeof(ProportionalVariableManager)
                .GetField("_variableB", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_manager, _variableB);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_managerObject);
        }

        [Test]
        public void SetVariableA_UpdatesVariableB()
        {
            // Act
            _manager.SetVariableA(50f);

            // Assert
            Assert.AreEqual(50f, _variableA.CurrentValue);
            Assert.AreEqual(25f, _variableB.CurrentValue); // 50% от максимума 50
        }

        [Test]
        public void SetVariableB_UpdatesVariableA()
        {
            // Act
            _manager.SetVariableB(25f);

            // Assert
            Assert.AreEqual(25f, _variableB.CurrentValue);
            Assert.AreEqual(50f, _variableA.CurrentValue); // 50% от максимума 100
        }

        [Test]
        public void UpdateFromA_SynchronizesCorrectly()
        {
            // Arrange
            _variableA.CurrentValue = 75f;

            // Act
            _manager.UpdateFromA();

            // Assert
            Assert.AreEqual(75f, _variableA.CurrentValue);
            Assert.AreEqual(37.5f, _variableB.CurrentValue, 0.001f);
        }

        [Test]
        public void UpdateFromB_SynchronizesCorrectly()
        {
            // Arrange
            _variableB.CurrentValue = 40f;

            // Act
            _manager.UpdateFromB();

            // Assert
            Assert.AreEqual(40f, _variableB.CurrentValue);
            Assert.AreEqual(80f, _variableA.CurrentValue, 0.001f);
        }


        [Test]
        public void NormalizedValues_ShouldBeEqual_AfterSynchronization()
        {
            // Arrange
            _variableA.CurrentValue = 60f; // 60% от 100

            // Act
            _manager.UpdateFromA();

            // Assert
            Assert.AreEqual(0.6f, _variableA.NormalizedValue, 0.001f);
            Assert.AreEqual(0.6f, _variableB.NormalizedValue, 0.001f);
        }

        [Test]
        public void DifferentRanges_NormalizedValuesRemainEqual()
        {
            // Arrange (замена переменных с разными диапазонами)
            var newVariableA = new ProportionalVariable(200f, 100f, 150f); // 50% нормализованное
            var newVariableB = new ProportionalVariable(10f, 0f, 5f); // 50% нормализованное

            typeof(ProportionalVariableManager)
                .GetField("_variableA", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_manager, newVariableA);

            typeof(ProportionalVariableManager)
                .GetField("_variableB", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_manager, newVariableB);

            // Act
            _manager.UpdateFromA();

            // Assert
            Assert.AreEqual(newVariableA.NormalizedValue, newVariableB.NormalizedValue, 0.001f);
        }

        [Test]
        public void ExtremumValues_SynchronizeCorrectly()
        {
            // Тест минимальных значений
            _manager.SetVariableA(0f);
            Assert.AreEqual(0f, _variableB.CurrentValue);
            Assert.AreEqual(0f, _variableA.NormalizedValue);
            Assert.AreEqual(0f, _variableB.NormalizedValue);

            // Тест максимальных значений
            _manager.SetVariableA(100f);
            Assert.AreEqual(50f, _variableB.CurrentValue);
            Assert.AreEqual(1f, _variableA.NormalizedValue, 0.001f);
            Assert.AreEqual(1f, _variableB.NormalizedValue, 0.001f);
        }
    }
}
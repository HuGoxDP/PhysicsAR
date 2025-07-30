using _Project.Scripts.Architecture.Scenario.FrictionScenario;
using NUnit.Framework;

namespace _Project.Scripts.Tests.Runtime.Tests
{
    public class ProportionalVariableTests
    {
        private ProportionalVariable _proportionalVariable;

        [SetUp]
        public void Setup()
        {
            _proportionalVariable = new ProportionalVariable(100f, 0f, 50f);
        }

        [Test]
        public void CurrentValue_WhenSetWithinRange_SetsCorrectValue()
        {
            // Act
            _proportionalVariable.CurrentValue = 75f;

            // Assert
            Assert.AreEqual(75f, _proportionalVariable.CurrentValue);
        }


        [Test]
        public void CurrentValue_WhenSetBelowMin_ClampsToMin()
        {
            // Act
            _proportionalVariable.CurrentValue = -10f;

            // Assert
            Assert.AreEqual(0f, _proportionalVariable.CurrentValue);
        }


        [Test]
        public void CurrentValue_WhenSetAboveMax_ClampsToMax()
        {
            // Act
            _proportionalVariable.CurrentValue = 150f;

            // Assert
            Assert.AreEqual(100f, _proportionalVariable.CurrentValue);
        }


        [Test]
        public void NormalizedValue_ReturnsCorrectValue()
        {
            // Arrange
            _proportionalVariable.CurrentValue = 75f;

            // Act
            float normalized = _proportionalVariable.NormalizedValue;

            // Assert
            Assert.AreEqual(0.75f, normalized, 0.001f);
        }

        [Test]
        public void SetFromNormalized_SetsCorrectValue()
        {
            // Act
            _proportionalVariable.SetFromNormalized(0.5f);

            // Assert
            Assert.AreEqual(50f, _proportionalVariable.CurrentValue, 0.001f);
        }

        [Test]
        public void NormalizedValue_WithNonZeroMinimum_CalculatesCorrectly()
        {
            // Arrange
            var variable = new ProportionalVariable(100f, 50f, 75f);

            // Act
            float normalized = variable.NormalizedValue;

            // Assert
            Assert.AreEqual(0.5f, normalized, 0.001f);
        }

        [Test]
        public void SetFromNormalized_WithNonZeroMinimum_SetsCorrectValue()
        {
            // Arrange
            var variable = new ProportionalVariable(100f, 50f, 50f);

            // Act
            variable.SetFromNormalized(0.5f);

            // Assert
            Assert.AreEqual(75f, variable.CurrentValue, 0.001f);
        }
    }
}
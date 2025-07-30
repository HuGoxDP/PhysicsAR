using System;
using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.FrictionScenario.Util
{
    public interface IDynamometerAnimation
    {
        void Setup(Transform targetObject, AnimationPoints points);
        void Animate(float normalizedValue);
    }

    public class HookAnimation : IDynamometerAnimation
    {
        private Transform _hook;
        private AnimationPoints _points;

        public void Setup(Transform targetObject, AnimationPoints points)
        {
            _hook = targetObject;
            _points = points;
        }

        public void Animate(float normalizedValue)
        {
            _hook.position = Vector3.Lerp(_points.StartPoint.position, _points.EndPoint.position, normalizedValue);
        }
    }

    public class BodyAnimation : IDynamometerAnimation
    {
        private Transform _body;
        private AnimationPoints _points;

        public void Setup(Transform targetObject, AnimationPoints points)
        {
            _body = targetObject;
            _points = points;
        }

        public void Animate(float normalizedValue)
        {
            _body.position = Vector3.Lerp(_points.StartPoint.position, _points.EndPoint.position, normalizedValue);
        }
    }

    [Serializable]
    public class AnimationPoints
    {
        public Transform StartPoint;
        public Transform EndPoint;
    }

    public class DynamometerView : MonoBehaviour
    {
        private const float MAXVALUE = 10f;
        [SerializeField] private Transform _hookPointer;
        [SerializeField] private Transform _bodyPointer;
        [SerializeField] private AnimationPoints _hookPoints;
        [SerializeField] private AnimationPoints _bodyPoints;

        private IDynamometerAnimation _currentAnimation;
        private Transform _currentAnimationTarget;
        private AnimationPoints _currentPoints;

        public void SetDynamometerValue(float value)
        {
            float normalizedValue = Mathf.Clamp01(value / MAXVALUE);
            _currentAnimation?.Animate(normalizedValue);
        }


        public void SetHookAnimationMode()
        {
            _currentAnimation = new HookAnimation();
            _currentAnimationTarget = _hookPointer;
            _currentPoints = _hookPoints;
            _currentAnimation.Setup(_currentAnimationTarget, _currentPoints);
        }

        public void SetBodyAnimationMode()
        {
            _currentAnimation = new BodyAnimation();
            _currentAnimationTarget = _bodyPointer;
            _currentPoints = _bodyPoints;
            _currentAnimation.Setup(_currentAnimationTarget, _currentPoints);
        }
    }
}
using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.Core
{
    public abstract class AnimationScenario : BaseScenario
    {
        [SerializeField] protected float _animationSpeed = 1f;
        [SerializeField] protected Animator _animator;
        [SerializeField] protected string _animationClip;


        public virtual void OnInteractPlay()
        {
            if (_animator != null)
            {
                _animator.enabled = true;
                _animator.Play(_animationClip);
            }
        }

        public virtual void OnInteractStop()
        {
            if (_animator != null)
            {
                _animator.enabled = false;
            }
        }

        public override void Enable()
        {
            base.Enable();
            if (IsAutoPlay)
            {
                _animator?.SetFloat("Speed", _animationSpeed);
                _animator?.Play(_animationClip);
            }
        }

        public override void Disable()
        {
            base.Disable();

            if (_animator != null)
            {
                _animator.enabled = false;
            }
        }
    }
}
using System;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Architecture.UI.Components
{
    public class UntargetUIAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform _animatedContent;
        [SerializeField] private float _animationDuration = 0.1f;
        [SerializeField] private float _hiddenPositionX = -800f;
        [SerializeField] private float _visiblePositionX = 0f;
        [SerializeField] private Ease _animationEase = Ease.InOutSine;

        private bool _isAnimating;

        public bool IsAnimating => _isAnimating;

        private void OnDestroy()
        {
            _animatedContent?.DOKill();
        }

        public void ShowContent(Action onComplete = null)
        {
            if (_isAnimating || _animatedContent == null) return;

            _isAnimating = true;
            _animatedContent.gameObject.SetActive(true);
            _animatedContent.DOKill();
            _animatedContent.DOAnchorPosX(_visiblePositionX, _animationDuration)
                .SetEase(_animationEase)
                .OnComplete(() =>
                    {
                        _isAnimating = false;
                        onComplete?.Invoke();
                    }
                );
        }

        public void HideContent(Action onComplete = null)
        {
            if (_isAnimating || _animatedContent == null) return;

            _isAnimating = true;
            _animatedContent.DOKill();
            _animatedContent.DOAnchorPosX(_hiddenPositionX, _animationDuration)
                .SetEase(_animationEase)
                .OnComplete(() =>
                    {
                        _isAnimating = false;
                        _animatedContent.gameObject.SetActive(false);
                        onComplete?.Invoke();
                    }
                );
        }
    }
}
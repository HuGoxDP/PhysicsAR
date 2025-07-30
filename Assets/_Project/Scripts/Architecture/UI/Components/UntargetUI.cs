using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Architecture.UI.Components
{
    [RequireComponent(typeof(ItemFactory))]
    [RequireComponent(typeof(UntargetUIAnimator))]
    public class UntargetUI : BaseUI
    {
        [SerializeField] private Button _mainButton;

        [SerializeField] private UntargetUIAnimator _uiAnimator;
        [SerializeField] private ItemFactory _itemFactory;

        private readonly List<Item> _items = new();
        private bool _isContentVisible;

        [Inject] private TargetDataList _targetDataList;

        private void Awake()
        {
            ValidateReferences();
            _mainButton?.onClick.AddListener(ShowHideController);
        }

        private void Start()
        {
            LoadItems();
        }

        private void OnDestroy()
        {
            _mainButton?.onClick.RemoveListener(ShowHideController);
        }

        private void ValidateReferences()
        {
            if (_mainButton == null)
                Debug.LogError("Main button not assigned!", this);
            if (_uiAnimator == null)
            {
                Debug.LogError("UIAnimator component not assigned!", this);
                _uiAnimator = GetComponent<UntargetUIAnimator>();
            }

            if (_itemFactory == null)
            {
                Debug.LogError("ItemFactory component not assigned!", this);
                _itemFactory = GetComponent<ItemFactory>();
            }
        }

        private void LoadItems()
        {
            var newItems = _itemFactory.CreateItemsFromTargetData(_targetDataList.DataList);
            _items.AddRange(newItems);
        }

        private void ShowHideController()
        {
            if (_uiAnimator.IsAnimating) return;

            if (_isContentVisible)
                Hide();
            else
                Show();
        }

        private void Show()
        {
            _isContentVisible = true;
            _uiAnimator.ShowContent();
        }

        private void Hide()
        {
            _isContentVisible = false;
            _uiAnimator.HideContent();
        }

        public override void Disable()
        {
            Hide();
            base.Disable();
        }
    }
}
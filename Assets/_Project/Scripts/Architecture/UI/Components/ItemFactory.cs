using System.Collections.Generic;
using _Project.Scripts.Architecture.Core;
using _Project.Scripts.Architecture.ScenarioDataTransfer;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Architecture.UI.Components
{
    public class ItemFactory : MonoBehaviour
    {
        [SerializeField] private GameObject _itemPrefab;
        [SerializeField] private RectTransform _itemContainer;

        [Inject] private ScenarioData _scenarioData;
        [Inject] private SceneLoader _sceneLoader;


        public List<Item> CreateItemsFromTargetData(List<ImageTargetData> targetDataList)
        {
            if (targetDataList == null || _itemPrefab == null || _itemContainer == null)
                return new List<Item>();

            List<Item> items = new List<Item>();

            foreach (var targetData in targetDataList)
            {
                if (targetData == null) continue;

                var item = CreateItem(targetData);
                if (item != null)
                    items.Add(item);
            }

            return items;
        }

        private Item CreateItem(ImageTargetData targetData)
        {
            var itemGameObject = Instantiate(_itemPrefab, _itemContainer);
            return itemGameObject.GetComponent<Item>()?.Init(targetData, _scenarioData, _sceneLoader);
        }
    }
}
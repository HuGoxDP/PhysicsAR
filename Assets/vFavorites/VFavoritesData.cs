#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Type = System.Type;
using static VFavorites.VFavoritesState;
using static VFavorites.Libs.VUtils;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


namespace VFavorites
{
    public class VFavoritesData : ScriptableObject
    {
        public List<Page> pages = new List<Page>();

        public float rowScale = 1;

        public Page curPage
        {
            get
            {
                while (curPageIndex >= pages.Count - 1)
                    pages.Add(new Page("Page " + (pages.Count + 1)));

                return pages[curPageIndex];
            }
        }

        public int curPageIndex
        {
            get => VFavoritesState.instance.curPageIndex;
            set => VFavoritesState.instance.curPageIndex = value;
        }


        [Serializable]
        public class Page
        {
            public List<Item> items = new List<Item>();

            public string name = "";
            public int _id = 0;


            [NonSerialized] public List<float> _rowGaps = new List<float>();

            public Page(string name) => this.name = name;

            public List<float> rowGaps
            {
                get
                {
                    if (_rowGaps == null)
                        _rowGaps = new List<float>();

                    while (_rowGaps.Count < items.Count + 1) _rowGaps.Add(0);
                    while (_rowGaps.Count > items.Count + 1) _rowGaps.RemoveLast();

                    return _rowGaps;
                }
            }


            public float scrollPos
            {
                get => state.scrollPos;
                set => state.scrollPos = value;
            }

            public long lastItemSelectTime_ticks
            {
                get => state.lastItemSelectTime_ticks;
                set => state.lastItemSelectTime_ticks = value;
            }

            public long lastItemDragTime_ticks
            {
                get => state.lastItemDragTime_ticks;
                set => state.lastItemDragTime_ticks = value;
            }


            public PageState state
            {
                get
                {
                    if (!VFavoritesState.instance.pageStates_byPageId.ContainsKey(id))
                        VFavoritesState.instance.pageStates_byPageId[id] = new PageState();

                    return VFavoritesState.instance.pageStates_byPageId[id];
                }
            }

            public int id
            {
                get
                {
                    if (_id == 0)
                        _id = Random.value.GetHashCode();

                    return _id;
                }
            }
        }

        [Serializable]
        public class Item
        {
            public GlobalID globalId;
            public string _typeString;
            public Object _obj;


            public bool isSceneGameObject;
            public bool isFolder;
            public bool isAsset;
            public int _id = 0;


            public Item(Object o)
            {
                globalId = o.GetGlobalID();


                isSceneGameObject = o is GameObject go && go.scene.rootCount != 0;
                isFolder = AssetDatabase.IsValidFolder(o.GetPath());
                isAsset = !isSceneGameObject && !isFolder;

                _typeString = o.GetType().AssemblyQualifiedName;

                _name = o.name;
            }


            public Type type => Type.GetType(_typeString) ?? typeof(DefaultAsset);

            public Object obj => _obj != null ? _obj : (_obj = globalId.GetObject());


            public bool isLoadable => obj != null;

            public bool isDeleted
            {
                get
                {
                    if (!isSceneGameObject)
                        return !isLoadable;

                    if (isLoadable)
                        return false;

                    if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(globalId.guid.ToPath()))
                        return true;

                    for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                        if (EditorSceneManager.GetSceneAt(i).path == globalId.guid.ToPath())
                            return true;

                    return false;
                }
            }

            public string assetPath => globalId.guid.ToPath();


            public string name
            {
                get
                {
                    if (!isLoadable) return _name;

                    if (assetPath.GetExtension() == ".cs")
                        _name = obj.name.Decamelcase();
                    else
                        _name = obj.name;

                    return _name;
                }
            }

            public string _name
            {
                get => state._name;
                set => state._name = value;
            }

            public string sceneGameObjectIconName
            {
                get => state.sceneGameObjectIconName;
                set => state.sceneGameObjectIconName = value;
            }

            public long lastSelectTime_ticks
            {
                get => state.lastSelectTime_ticks;
                set => state.lastSelectTime_ticks = value;
            }

            public bool isSelected
            {
                get => state.isSelected;
                set => state.isSelected = value;
            }


            public ItemState state
            {
                get
                {
                    if (!VFavoritesState.instance.itemStates_byItemId.ContainsKey(id))
                        VFavoritesState.instance.itemStates_byItemId[id] = new ItemState();

                    return VFavoritesState.instance.itemStates_byItemId[id];
                }
            }

            public int id
            {
                get
                {
                    if (_id == 0)
                        _id = Random.value.GetHashCode();

                    return _id;
                }
            }
        }
    }
}
#endif
#if UNITY_EDITOR
using System;
using UnityEditor;
using static VFavorites.Libs.VUtils;


namespace VFavorites
{
    [FilePath("Library/vFavorites State.asset", FilePathAttribute.Location.ProjectFolder)]
    public class VFavoritesState : ScriptableSingleton<VFavoritesState>
    {
        public int curPageIndex;

        public SerializableDictionary<int, PageState>
            pageStates_byPageId = new SerializableDictionary<int, PageState>();

        public SerializableDictionary<int, ItemState>
            itemStates_byItemId = new SerializableDictionary<int, ItemState>();


        public static void Save() => instance.Save(true);


        [Serializable]
        public class PageState
        {
            public long lastItemSelectTime_ticks;
            public long lastItemDragTime_ticks;

            public float scrollPos;
        }

        [Serializable]
        public class ItemState
        {
            public string _name;

            public string sceneGameObjectIconName;

            public long lastSelectTime_ticks;
            public bool isSelected;
        }
    }
}
#endif
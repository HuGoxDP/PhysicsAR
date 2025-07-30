using _Project.Scripts.Architecture.UI.Components;
using _Project.Scripts.Architecture.UI.Core;

namespace _Project.Scripts.Architecture.UI.Interfaces
{
    public interface IUIFactory
    {
        BaseUI GetUI(UIType uiType);
        void RegisterUI(UIType uiType, BaseUI uiImplementation);
    }
}
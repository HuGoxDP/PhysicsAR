using _Project.Scripts.Architecture.UI.Components;
using _Project.Scripts.Architecture.UI.Core;

namespace _Project.Scripts.Architecture.UI.Interfaces
{
    public interface IUIManager
    {
        BaseUI CurrentUI { get; }
        void SwitchToUI(UIType uiType);
    }
}
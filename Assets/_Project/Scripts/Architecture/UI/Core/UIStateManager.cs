using _Project.Scripts.Architecture.UI.Components;

namespace _Project.Scripts.Architecture.UI.Core
{
    public class UIStateManager
    {
        private BaseUI _currentUI;
        public BaseUI CurrentUI => _currentUI;

        public void SetCurrentUI(BaseUI ui)
        {
            _currentUI = ui;
        }
    }
}
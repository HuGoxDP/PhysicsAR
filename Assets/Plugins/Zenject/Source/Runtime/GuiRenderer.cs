using UnityEngine;

namespace Zenject
{
    public class GuiRenderer : MonoBehaviour
    {
        GuiRenderableManager _renderableManager;

        public void OnGUI()
        {
            _renderableManager.OnGui();
        }

        [Inject]
        void Construct(GuiRenderableManager renderableManager)
        {
            _renderableManager = renderableManager;
        }
    }
}
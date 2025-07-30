using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.Core
{
    public abstract class BaseScenario : MonoBehaviour
    {
        protected IDisplayableInfo DisplayableInfo;
        public bool IsAutoPlay { get; protected set; }
        public IDisplayableInfo GetDisplayableInfo() => DisplayableInfo;


        public void SetDisplayableInfo(IDisplayableInfo displayableInfo)
        {
            DisplayableInfo = displayableInfo;
        }

        public void SetAutoPlay(bool autoPlay)
        {
            IsAutoPlay = autoPlay;
        }

        public virtual void Enable()
        {
            gameObject.SetActive(true);
        }

        public virtual void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}
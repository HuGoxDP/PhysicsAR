using _Project.Scripts.Architecture.UI.Interfaces;
using UnityEngine;

namespace _Project.Scripts.Architecture.UI.Components
{
    public abstract class BaseUI : MonoBehaviour, IUI
    {
        public virtual void Enable() => gameObject.SetActive(true);
        public virtual void Disable() => gameObject.SetActive(false);
    }
}
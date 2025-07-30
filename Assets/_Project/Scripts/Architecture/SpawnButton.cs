using System;
using UnityEngine;

namespace _Project.Scripts.Architecture
{
    public class SpawnButton : MonoBehaviour, ISpawnButton
    {
        public event Action OnButtonClicked;

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void OnClick()
        {
            OnButtonClicked?.Invoke();
        }
    }
}
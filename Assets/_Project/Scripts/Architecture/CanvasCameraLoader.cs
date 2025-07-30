using UnityEngine;
using Zenject;

namespace _Project.Scripts.Architecture
{
    public class CanvasCameraLoader : MonoBehaviour
    {
        [Inject] private Camera _camera;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.worldCamera = _camera ?? Camera.main;
        }
    }
}
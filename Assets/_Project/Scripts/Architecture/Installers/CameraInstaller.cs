using UnityEngine;
using Zenject;

namespace _Project.Scripts.Architecture.Installers
{
    public class CameraInstaller : MonoInstaller
    {
        [SerializeField] private Camera _camera;

        public override void InstallBindings()
        {
            if (_camera != null)
            {
                Container.Bind<Camera>()
                    .FromInstance(_camera)
                    .AsSingle();
            }
            else
            {
                Debug.LogError("Camera is not assigned in inspector!");
            }
        }
    }
}
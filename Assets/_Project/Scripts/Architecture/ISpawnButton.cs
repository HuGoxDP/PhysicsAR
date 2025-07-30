using System;

namespace _Project.Scripts.Architecture
{
    public interface ISpawnButton : IToggleable
    {
        event Action OnButtonClicked;
    }
}
using System;
using System.Collections.Generic;
using _Project.Scripts.Architecture.UI.Components;
using _Project.Scripts.Architecture.UI.Interfaces;

namespace _Project.Scripts.Architecture.UI.Core
{
    public class UIFactory : IUIFactory
    {
        private readonly Dictionary<UIType, BaseUI> _registeredUIs = new();

        public void RegisterUI(UIType uiType, BaseUI uiImplementation)
        {
            _registeredUIs[uiType] = uiImplementation;
        }

        public BaseUI GetUI(UIType uiType)
        {
            if (_registeredUIs.TryGetValue(uiType, out var ui))
                return ui;

            throw new ArgumentOutOfRangeException(
                nameof(uiType),
                $"UI type {uiType} not registered"
            );
        }
    }
}
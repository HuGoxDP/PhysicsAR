using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Architecture.Scenario.Core
{
    public abstract class InteractiveScenario : BaseScenario
    {
        protected readonly List<IObservableFieldComponent> _observableFieldComponentList = new();
        public List<IObservableFieldComponent> GetDisplayableComponents() => _observableFieldComponentList;

        public List<IObservableFieldComponent> GetEditableComponents() =>
            _observableFieldComponentList.FindAll(component =>
                component.ComponentType != ObservableFieldComponentType.None
            );

        protected void AddObservableFieldComponent(IObservableFieldComponent observableFieldComponent)
        {
            if (observableFieldComponent == null)
            {
                Debug.LogError("ObservableFieldComponent is null");
                return;
            }

            _observableFieldComponentList.Add(observableFieldComponent);
        }

        public abstract void OnInteractPlay();

        public abstract void OnInteractStop();
    }
}
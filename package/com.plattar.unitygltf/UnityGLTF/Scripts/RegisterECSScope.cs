using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityGLTF
{
    /// <summary>
    /// RAII wrapper for enabling a subtree while disabling all components.
    /// This is necessary to enable all MultiSpatialGameObjectTracker components while preventing
    /// OnEnable / OnDisable from being triggered unnecessarily
    /// </summary>
    public class RegisterECSScope : IDisposable
    {
        private static (MonoBehaviour, bool)[] disabledComponents = null;

        private static bool FilterComponent(MonoBehaviour component)
        {
            return component
					&& component.GetComponent<CanvasRenderer>() == null
					&& component.GetComponent<UIBehaviour>() == null;
        }

        private static bool TryDisableChildComponents(GameObject obj)
        {
            // If we already disabled a subtree then return null
            if (disabledComponents != null)
            {
                return false;
            }
            
            disabledComponents = obj.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(FilterComponent)
                .Select(component => (component, component.enabled))
                .ToArray();
                
            foreach (var (component, _) in disabledComponents)
            {
                component.enabled = false;
            }
            
            return true;
        }

        private static void RestoreChildComponentActiveStates()
        {
            foreach (var (component, wasEnabled) in disabledComponents)
            {
                component.enabled = wasEnabled;
            }
            
            disabledComponents = null;
        }

        private readonly GameObject _gameObject;
        private bool _wasGameObjectActive = false;
        private bool _wereChildComponentsDisabled = false;
        private bool _disposed = false;

        public RegisterECSScope(GameObject gameObject)
        {
            _gameObject = gameObject;
            _wasGameObjectActive = _gameObject.activeSelf;
            if (!_wasGameObjectActive)
            {
                _wereChildComponentsDisabled = TryDisableChildComponents(_gameObject);
                _gameObject.SetActive(true);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (!_wasGameObjectActive)
                {
                    _gameObject.SetActive(false);
                    if (_wereChildComponentsDisabled)
                    {
                        RestoreChildComponentActiveStates();
                    }
                }
                _disposed = true;
            }
        }
    }
}

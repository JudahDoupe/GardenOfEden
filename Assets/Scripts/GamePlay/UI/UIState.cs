using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIState
{
    public class UiState: MonoBehaviour
    {
        public virtual IEnumerable<UnityEngine.Object> Reload() { return null; }
    }
    
    public static class UiStateMachine
    {
        private static UiState _currentUiState;
        
        public static void SetState(UiState uiState)
        {
            SetEnabled(_currentUiState, false);
            _currentUiState = uiState;
            SetEnabled(_currentUiState, true);
        }

        private static void SetEnabled(UiState uiState, bool enabled)
        {
            if (uiState == null) return;

            var panel = uiState.GetComponent<PanelRenderer>();
            var eventSystem = uiState.GetComponent<UIElementsEventSystem>();

            panel.visualTree.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
            panel.enabled = enabled;
            eventSystem.enabled = enabled;

            if (enabled)
            {
                uiState.Reload();
            }
        }
    }
}

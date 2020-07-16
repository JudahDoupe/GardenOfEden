using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIState
{
    public enum StateType
    {
        None,
        Evolution,
        PlantDetails
    }

    public class State: MonoBehaviour
    {
        public virtual IEnumerable<UnityEngine.Object> Reload() { return null; }
    }
    
    public class StateMachine
    {
        private Dictionary<StateType, State> _states;
        private State _currentState;

        public StateMachine()
        {
            _states = new Dictionary<StateType, State>();
        }
        public void AddState(StateType type, State state)
        {
            _states.Add(type, state);
            SetEnabled(state, false);
        }
        public void SetState(StateType type)
        {
            SetEnabled(_currentState, false);
            _states.TryGetValue(type, out _currentState);
            SetEnabled(_currentState, true);
        }
        public bool IsState(StateType type)
        {
            return _states[type] == _currentState;
        }

        private void SetEnabled(State state, bool enabled)
        {
            if (state == null) return;

            var panel = state.GetComponent<PanelRenderer>();
            var eventSystem = state.GetComponent<UIElementsEventSystem>();

            panel.visualTree.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
            panel.enabled = enabled;
            eventSystem.enabled = enabled;

            if (enabled)
            {
                state.Reload();
            }
        }
    }
}

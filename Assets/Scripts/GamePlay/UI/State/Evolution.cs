using System;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIState
{
    public class Evolution : State
    {
        private VisualElement _root;
        void OnEnabled()
        {
            _root = GetComponent<PanelRenderer>().visualTree;

            RegisterClickedAction(_root.Q<Button>("leaves"), ClickLeaves);
        }
        private void ClickLeaves()
        {
            Debug.Log("You clicked the leaves");
        }
    }
}

using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIState
{
    public class PlantInspectionUi : UiState
    {
        public bool IsActive { get; private set; }
        private Plant _inspectedPlant;
        private VisualElement _root;
        
        public override IEnumerable<UnityEngine.Object> Reload()
        {
            _root = GetComponent<PanelRenderer>().visualTree;
            InspectPlant(_inspectedPlant);
            SetInactive();
            return null;
        }

        public void InspectPlant(Plant plant)
        {
            _inspectedPlant = plant;
            if (plant == null)
            {
                //TODO: hide elements                
            }
            else
            {
                //TODO: configure UI
            }
            //root.Q<Button>(name: "close-button").clickable.clicked += CloseMenu;
            _root.Q<Label>(name: "SpeciesName").text = _inspectedPlant.PlantDna.Name;
        }

        private void SetActive()
        {
            IsActive = true;
        }
        
        private void SetInactive()
        {
            IsActive = false;
        }
        
        private void CloseMenu()
        {
            UiStateMachine.SetState(null);
        }
    }
}

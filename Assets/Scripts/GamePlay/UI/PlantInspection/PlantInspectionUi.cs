using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEngine.UIElements;

namespace UIState
{
    public class PlantInspectionUi : UiState
    {
        public bool IsActive { get; private set; }
        private Plant _inspectedPlant;
        
        public override IEnumerable<UnityEngine.Object> Reload()
        {
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
            var root = GetComponent<PanelRenderer>().visualTree;
            root.Q<Button>(name: "close-button").clickable.clicked += CloseMenu;
            root.Q<Label>(name: "species-name").text = _inspectedPlant.PlantDna.Name;
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

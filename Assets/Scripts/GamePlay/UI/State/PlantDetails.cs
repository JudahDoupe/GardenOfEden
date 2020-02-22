using System;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIState
{
    public class PlantDetails : State
    {
        public override void Enable()
        {
            var plant = DI.CameraController.PrimaryFocus.Object?.GetComponent<Plant>();

            if (plant == null)
            {
                CloseMenu();
            }
            else
            {
                var root = GetComponent<PanelRenderer>().visualTree;
                root.Q<Button>(name: "close-button").clickable.clicked += CloseMenu;
                root.Q<Label>(name: "species-name").text = plant.Dna.Name;
            }
        }

        private void CloseMenu()
        {
            DI.UIController.State.SetState(StateType.None);
        }
    }
}

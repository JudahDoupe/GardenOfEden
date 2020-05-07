using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CameraState
{
    public class Species : ICameraState
    {
        private Plant focusedPlant;
        private bool isActive;
        private Bounds bounds;
        public Species()
        {
            DI.GrowthService.NewPlantSubject.Subscribe(NewPlantAction);
        }

        public void TransitionTo()
        {
            DI.CameraController.MoveSpeed = 0.25f;
            DI.CameraController.LookSpeed = 0.75f;
            isActive = true;
            focusedPlant = GameObject.FindObjectsOfType<Plant>().First();
            bounds = focusedPlant.transform.GetBounds();
            DI.CameraController.SecondaryFocus.Object = null;
            DI.CameraController.PrimaryFocus.Object = null;
        }

        public void TransitionAway() 
        {
            isActive = false;
        }

        public void Update()
        {
            var pDistance = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z, 4) * (145f / Camera.main.fieldOfView) * 0.7f;
            var pPosition = bounds.center;
            var direction = Camera.main.transform.position - pPosition;
            direction.y = pDistance / 2.5f;
            direction = direction.normalized;

            DI.CameraController.TargetPosition = pPosition + (direction * pDistance);
            DI.CameraController.TargetFocusPosition = pPosition;
        }

        private void NewPlantAction(Plant plant)
        {
            if (!isActive || plant.Dna.SpeciesId != focusedPlant.Dna.SpeciesId) return;

            bounds.Encapsulate(plant.transform.position);
        }
    }
}

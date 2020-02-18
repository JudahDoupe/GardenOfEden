﻿using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace CameraState
{
    public class Inspection : ICameraState
    {
        private Stopwatch activityTimer = new Stopwatch();
        private TimeSpan timeout = TimeSpan.FromSeconds(10);

        public void TransitionTo()
        {
            DI.CameraController.MoveSpeed = 0.5f;
            DI.CameraController.LookSpeed = 0.75f;
            DI.CameraController.SecondaryFocus.Object = null;
            activityTimer.Start();
        }

        public void TransitionAway() 
        {
            CloseEvolutionMenu();
            activityTimer.Stop();
        }

        public void Update()
        {
            var movementVector = Camera.main.transform.TransformVector(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));
            if (movementVector.magnitude > 0.1f && DI.UIController.State != UIController.UIState.Evolution)
            {
                var target = GetGroundPosition(DI.CameraController.TargetPosition + movementVector * DI.CameraController.MoveSpeed);
                target.y = DI.CameraController.PrimaryFocus.GetPosition().y;
                DI.CameraController.TargetPosition = target;
                DI.CameraController.TargetFocusPosition = DI.CameraController.TargetPosition + movementVector;

                var plant = GetNearestPlantInDirection(Camera.main.transform.forward)?.transform;
                if (plant != null)
                {
                    DI.CameraController.PrimaryFocus.Object = plant;
                    DI.CameraController.PrimaryFocus.HorizontalOffsetRatio = 0;
                }

                activityTimer.Restart();
            }
            else
            {
                var focusBounds = DI.CameraController.PrimaryFocus.Object.GetBounds();
                var focusDistance = Mathf.Max(focusBounds.extents.x, focusBounds.extents.y, focusBounds.extents.z) * (145f / Camera.main.fieldOfView);
                var focusPosition = DI.CameraController.PrimaryFocus.GetPositionWithOffset(focusDistance);
                var target = GetGroundPosition(Camera.main.transform.position);
                target.y = focusPosition.y;
                var focusDirection = (target - focusPosition).normalized;
                DI.CameraController.TargetPosition = focusPosition + (focusDirection * focusDistance);
                DI.CameraController.TargetFocusPosition = focusPosition;

                if (TimeSpan.FromMilliseconds(activityTimer.ElapsedMilliseconds) > timeout)
                {
                    activityTimer.Stop();
                    DI.CameraController.State.Set(CameraStateType.Cinematic);
                }
            }

            if (Input.GetKeyDown(KeyCode.E) && DI.UIController.State != UIController.UIState.Evolution)
            {
                OpenEvolutionMenu();
            }
            if (Input.GetKeyDown(KeyCode.Escape) && DI.UIController.State == UIController.UIState.Evolution)
            {
                CloseEvolutionMenu();
            }
        }
        private Plant GetNearestPlantInDirection(Vector3 direction)
        {
            var radius = 25;
            var forwardOffset = 2;
            var position = GetGroundPosition(Camera.main.transform.position + direction * (radius + forwardOffset));
            return Physics.OverlapSphere(position, radius)
                                .Select(x => x.GetComponentInParent<Plant>())
                                .Where(x => x != null)
                                .Distinct()
                                .Aggregate((curMin, x) => curMin == null || PlantDistanceFromCenter(x) < PlantDistanceFromCenter(curMin) ? x : curMin);
        }
        private float PlantDistanceFromCenter(Plant plant)
        {
            var a = plant.transform.position;
            var b = Camera.main.transform.position;
            return Vector3.Distance(a, b) * (Vector3.Angle(Camera.main.transform.forward, (a - b).normalized) / 2);
        }
        private Vector3 GetGroundPosition(Vector3 position)
        {
            return new Vector3(position.x, DI.LandService.SampleTerrainHeight(position), position.z);
        }

        private void OpenEvolutionMenu()
        {
            DI.UIController.SetState(UIController.UIState.Evolution);
            activityTimer.Stop();
        }

        private void CloseEvolutionMenu()
        {
            DI.UIController.SetState(UIController.UIState.None);
            activityTimer.Restart();
        }
    }
}

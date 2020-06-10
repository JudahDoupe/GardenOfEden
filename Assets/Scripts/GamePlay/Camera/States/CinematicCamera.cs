using System;
using System.Collections;
using UnityEngine;

namespace CameraState
{
    public class CinematicCamera : ICameraState
    {

        private bool isPrimaryFocusDrifting = true;
        private Plant returningFocus;
        private bool isActive;

        public CinematicCamera()
        {
            //DI.GrowthService.NewPlantSubject.Subscribe(NewPlantAction);
            DI.GameService.PointCapturedSubject.Subscribe(PointCapturedAction);
        }

        public void TransitionTo()
        {
            DI.CameraController.MoveSpeed = 0.25f;
            DI.CameraController.LookSpeed = 0.75f;
            DI.CameraController.SecondaryFocus.Object = DI.GameService.GetNearestGoal()?.transform;
            isActive = true;
        }

        public void TransitionAway() 
        {
            isActive = false;
        }

        public void Update()
        {
            if (DI.CameraController.PrimaryFocus.Object != null)
            {
                var focusBounds = DI.CameraController.PrimaryFocus.Object.GetBounds();
                var pDistance = Mathf.Max(focusBounds.extents.x, focusBounds.extents.y, focusBounds.extents.z) * (145f / Camera.main.fieldOfView);
                var pPosition = DI.CameraController.PrimaryFocus.GetPositionWithOffset(pDistance);
                var direction = (Camera.main.transform.position - pPosition).normalized;

                if (DI.CameraController.SecondaryFocus.Object != null)
                {
                    var sDistance = Vector3.Distance(DI.CameraController.PrimaryFocus.Object.transform.position, DI.CameraController.SecondaryFocus.Object.transform.position);
                    var sPosition = DI.CameraController.SecondaryFocus?.GetPositionWithOffset(sDistance) ?? pPosition;
                    direction = (pPosition - sPosition).normalized;
                }

                DI.CameraController.TargetPosition = pPosition + (direction * pDistance);
                DI.CameraController.TargetFocusPosition = pPosition;
            }
        }

        private void PointCapturedAction(CapturePoint cp)
        {
            if (!isActive) return;

            cp.StartCoroutine(this.PointCapturedActionAsync(cp));
        }
        private void NewPlantAction(Plant plant)
        {
            if (!isActive) return;

            var plantPos = plant.transform.position;
            var primaryPos = plantPos;
            if (DI.CameraController.PrimaryFocus.Object != null)
            {
                primaryPos = DI.CameraController.PrimaryFocus.Object.position;
            }
            var secondaryPos = plantPos;
            if (DI.CameraController.SecondaryFocus.Object != null)
            {
                secondaryPos = DI.CameraController.SecondaryFocus.Object.position;
            }

            if (isPrimaryFocusDrifting && Vector3.Distance(plantPos, secondaryPos) <= Vector3.Distance(primaryPos, secondaryPos))
            {
                if (isPrimaryFocusDrifting)
                {
                    DI.CameraController.PrimaryFocus.Object = plant.transform;
                    DI.CameraController.PrimaryFocus.RandomizeHorizontalOffsetRatio();
                }
                else
                {
                    returningFocus = plant;
                }
            }
        }

        private IEnumerator PointCapturedActionAsync(CapturePoint cp)
        {
            isPrimaryFocusDrifting = false;
            DI.CameraController.PrimaryFocus.Object = cp.transform;
            DI.CameraController.PrimaryFocus.HorizontalOffsetRatio = 0;
            DI.CameraController.SecondaryFocus.Object = DI.GameService.GetNearestGoal()?.transform;

            yield return new WaitForSeconds(5);

            DI.CameraController.PrimaryFocus.Object = returningFocus.transform;
            DI.CameraController.PrimaryFocus.RandomizeHorizontalOffsetRatio();
            isPrimaryFocusDrifting = true;
        }
    }
}

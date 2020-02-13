using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ICameraState
{
    void Transition();
    void Update();
}

public class BirdsEye : ICameraState
{
    public void Transition()
    {
        DI.CameraTransform.MoveSpeed = 1f;
        DI.CameraTransform.LookSpeed = 10;
        DI.CameraTransform.TargetPosition = DI.CameraFocus.PrimaryFocus.Position(0) + new Vector3(0, 200, -25);
        DI.CameraTransform.TargetFocusPosition = DI.CameraFocus.PrimaryFocus.Position(0);
        DI.CameraFocus.PrimaryFocus.IsDrifting = false;
        DI.CameraFocus.SecondaryFocus.IsDrifting = false;
    }

    public void Update() { }
}

public class Cinematic : ICameraState
{
    public Cinematic()
    {
        DI.GrowthService.NewPlantSubject.Subscribe(NewPlantAction);
        DI.GameService.PointCapturedSubject.Subscribe(PointCapturedAction);
    }

    public void Transition()
    {
        DI.CameraTransform.MoveSpeed = 0.25f;
        DI.CameraTransform.LookSpeed = 0.75f;
        DI.CameraFocus.PrimaryFocus.IsDrifting = true;
        DI.CameraFocus.SecondaryFocus.IsDrifting = true;
        DI.CameraFocus.SecondaryFocus.Object = DI.GameService.GetNearestGoal()?.transform;
    }

    public void Update()
    {
        if (DI.CameraFocus.PrimaryFocus.Object != null)
        {
            var focusBounds = DI.CameraFocus.PrimaryFocus.Object.GetBounds();
            var pDistance = Mathf.Max(focusBounds.extents.x, focusBounds.extents.y, focusBounds.extents.z) * (145f / Camera.main.fieldOfView);
            var pPosition = DI.CameraFocus.PrimaryFocus.Position(pDistance);
            var direction = (Camera.main.transform.position - pPosition).normalized;

            if (DI.CameraFocus.SecondaryFocus.Object != null)
            {
                var sDistance = Vector3.Distance(DI.CameraFocus.PrimaryFocus.Object.transform.position, DI.CameraFocus.SecondaryFocus.Object.transform.position);
                var sPosition = DI.CameraFocus.SecondaryFocus?.Position(sDistance) ?? pPosition;
                direction = (pPosition - sPosition).normalized;
            }

            DI.CameraTransform.TargetPosition = pPosition + (direction * pDistance);
            DI.CameraTransform.TargetFocusPosition = pPosition;
        }
    }

    private void PointCapturedAction(CapturePoint cp)
    {
        if (DI.CameraFocus.SecondaryFocus.IsDrifting)
        {
            DI.CameraFocus.HoldFocus(DI.CameraFocus.SecondaryFocus, cp.transform, TimeSpan.FromSeconds(5));
        }
    }
    private void NewPlantAction(Plant plant)
    {
        var plantPos = plant.transform.position;
        var primaryPos = DI.CameraFocus.PrimaryFocus.Object?.position ?? plantPos;
        var secondaryPos = DI.CameraFocus.SecondaryFocus.Object?.position ?? plantPos;
        if (DI.CameraFocus.PrimaryFocus.IsDrifting && Vector3.Distance(plantPos, secondaryPos) <= Vector3.Distance(primaryPos, secondaryPos))
        {
            DI.CameraFocus.PrimaryFocus.Object = plant.transform;
        }
    }
}

public class Inspection : ICameraState
{
    public void Transition()
    {
        DI.CameraTransform.MoveSpeed = 0.25f;
        DI.CameraTransform.LookSpeed = 0.75f;
        DI.CameraFocus.PrimaryFocus.IsDrifting = false;
        DI.CameraFocus.SecondaryFocus.IsDrifting = false;
        DI.CameraFocus.SecondaryFocus.Object = null;
    }

    public void Update()
    {
        var direction = Camera.main.transform.TransformVector(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));
        if (direction.magnitude > 0.1f)
        {
            var plant = GetNearestPlantInDirection(direction.normalized)?.transform;
            if(plant != null)
            {
                DI.CameraFocus.PrimaryFocus.Object = plant;
                var focusBounds = DI.CameraFocus.PrimaryFocus.Object.GetBounds();
                var pDistance = Mathf.Max(focusBounds.extents.x, focusBounds.extents.y, focusBounds.extents.z) * (145f / Camera.main.fieldOfView);
                var pPosition = DI.CameraFocus.PrimaryFocus.Position(pDistance);
                DI.CameraTransform.TargetPosition = pPosition + (direction * pDistance);
                DI.CameraTransform.TargetFocusPosition = pPosition;
            }
        }
    }

    private Plant GetNearestPlantInDirection(Vector3 direction)
    {
        var radius = 30;

        var nearPos = Camera.main.transform.position;
        var nearPlants = GetPlantsInRadius(nearPos, radius);
        var farPos = nearPos + direction * radius;
        var farPlants = GetPlantsInRadius(farPos, radius);

        var intersection = nearPlants.Intersect(farPlants).ToList();
        var currentPlant = DI.CameraFocus.PrimaryFocus.Object?.GetComponent<Plant>();
        if (currentPlant != null)
        {
            intersection.Remove(currentPlant);
        }
        Plant nearestPlant = intersection.FirstOrDefault();
        foreach (var plant in intersection)
        {
            if (Vector3.Distance(nearPos, plant.transform.position) < Vector3.Distance(nearPos, nearestPlant.transform.position))
            {
                nearestPlant = plant;
            }
        }
        
        return nearestPlant;
    }
    private Vector3 GetGroundPosition(Vector3 position)
    {
        return new Vector3(position.x, DI.LandService.SampleTerrainHeight(position), position.z);
    }
    private List<Plant> GetPlantsInRadius(Vector3 position, float radius)
    {
        var plants = Physics.OverlapSphere(GetGroundPosition(position), radius)
                            .Select(x => x.GetComponentInParent<Plant>())
                            .Distinct();
        return plants.ToList();
         
    }
}

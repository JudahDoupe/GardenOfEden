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
        var direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (direction.magnitude > 0.1f)
        {
            //Find plant of focused species
            //set focus primary focus to that plant
        }
    }
}

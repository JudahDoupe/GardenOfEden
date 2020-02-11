using UnityEngine;

public abstract class CameraState
{
    protected CameraTransform _transform;
    protected CameraFocus _focus;

    public CameraState(CameraTransform transform, CameraFocus focus)
    {
        _transform = transform;
        _focus = focus;
    }

    public abstract void Transition();
    public abstract void Update();
}

public class BirdsEye : CameraState
{
    public BirdsEye(CameraTransform transform, CameraFocus focus) : base(transform, focus) {}

    public override void Transition()
    {
        _transform.MoveSpeed = 1f;
        _transform.LookSpeed = 10;
        _transform.TargetPosition = new Vector3(0, 350, -100);
        _transform.TargetFocusPosition = _focus.PrimaryFocus.Position(0);
        _focus.PrimaryFocus.IsDrifting = false;
        _focus.SecondaryFocus.IsDrifting = false;
    }

    public override void Update() { }
}

public class Cinematic : CameraState
{
    public Cinematic(CameraTransform transform, CameraFocus focus) : base(transform, focus) { }

    public override void Transition()
    {
        _transform.MoveSpeed = 0.25f;
        _transform.LookSpeed = 0.75f;
        _focus.PrimaryFocus.IsDrifting = true;
        _focus.SecondaryFocus.IsDrifting = true;
    }

    public override void Update()
    {
        if (_focus.PrimaryFocus.Object != null)
        {
            var focusBounds = _focus.PrimaryFocus.Object.GetBounds();
            var pDistance = Mathf.Max(focusBounds.extents.x, focusBounds.extents.y, focusBounds.extents.z) * (145f / Camera.main.fieldOfView);
            var pPosition = _focus.PrimaryFocus.Position(pDistance);
            var direction = (Camera.main.transform.position - pPosition).normalized;

            if (_focus.SecondaryFocus.Object != null)
            {
                var sDistance = Vector3.Distance(_focus.PrimaryFocus.Object.transform.position, _focus.SecondaryFocus.Object.transform.position);
                var sPosition = _focus.SecondaryFocus?.Position(sDistance) ?? pPosition;
                direction = (pPosition - sPosition).normalized;
            }

            _transform.TargetPosition = pPosition + (direction * pDistance);
            _transform.TargetFocusPosition = pPosition;
        }
    }
}

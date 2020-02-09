using UnityEngine;

public class CameraController : MonoBehaviour
{

    public CameraMode CurrentMode = CameraMode.Cinematic;

    /* INNER MECHINATIONS */

    private CameraTransform _transform;
    private CameraFocus _focus;

    public enum CameraMode
    {
        Cinematic,
        BirdsEye,
    }

    private void Start()
    {
        _focus = FindObjectOfType<CameraFocus>();
        _transform = FindObjectOfType<CameraTransform>();
    }
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CurrentMode = CurrentMode == CameraMode.Cinematic ? CameraMode.BirdsEye : CameraMode.Cinematic;
        }

        switch (CurrentMode)
        {
            case CameraMode.Cinematic:
                UpdateCinematic();
                break;
            case CameraMode.BirdsEye:
                UpdateBirdsEye();
                break;
        }
    }

    private void UpdateCinematic()
    {
        _transform.MoveSpeed = 0.25f;
        _transform.LookSpeed = 0.75f;
        _focus.PrimaryFocus.IsDrifting = true;
        _focus.SecondaryFocus.IsDrifting = true;

        if (_focus.PrimaryFocus.Object != null)
        {
            var focusBounds = _focus.PrimaryFocus.Object.GetBounds();
            var pDistance = Mathf.Max(focusBounds.extents.x, focusBounds.extents.y, focusBounds.extents.z) * (145f / Camera.main.fieldOfView);
            var pPosition = _focus.PrimaryFocus.Position(pDistance);
            var direction = (transform.position - pPosition).normalized;

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

    private void UpdateBirdsEye()
    {
        _transform.MoveSpeed = 1f; //These values should only be set on transistion
        _transform.LookSpeed = 10;
        _focus.PrimaryFocus.IsDrifting = false;
        _focus.SecondaryFocus.IsDrifting = false;

        _transform.TargetPosition = new Vector3(0, 350, -100);
        _transform.TargetFocusPosition = _focus.PrimaryFocus.Position(0);
    }

}

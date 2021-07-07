using Unity.Mathematics;
using UnityEngine;

public class LandscapeCamera : MonoBehaviour
{
    [Header("Altitude")] 
    public float LerpSpeed = 2f;
    public float MaxAltitude = 3000;
    public float MinHeight = 100;
    public float MaxZoomSpeed = 15f;
    public float MinZoomSpeed = 15f;
    [Header("Movement")]
    public float MaxMovementSpeed = 30f;
    public float MinMovementSpeed = 30f;
    [Header("Rotation")]
    public Vector2 RotationSpeed;
    public float MaxAngle = 90;
    public float MinAngle = 60;
    [Header("FOV")]
    public float MaxFov = 30;
    public float MinFov = 60;

    public bool IsActive { get; private set; }

    private Transform _camera;
    private Transform _focus;

    private Coordinate _focusCoord;
    private float _altitude;

    public void Enable(Transform camera, Transform focus)
    {
        _camera = camera;
        _focus = focus;
        _focusCoord = new Coordinate(camera.position, Planet.LocalToWorld);
        _altitude = math.clamp(_focusCoord.Altitude, Singleton.Land.SampleHeight(_focusCoord) + MinHeight, MaxAltitude);
        _focusCoord.Altitude = _altitude;

        CameraUtils.SetState(new CameraState(camera, focus)
        {
            CameraParent = focus,
            CameraLocalPosition = new Vector3(0,0,-1), 
            FocusParent = Planet.Transform,
            FocusLocalPosition = _focusCoord.LocalPlanet,
        });
        CameraUtils.TransitionState(GetTargetState(camera, focus, _focusCoord), () =>
        {
            IsActive = true;
            Cursor.lockState = CursorLockMode.Locked;
        });

    }

    public void Disable()
    {
        Cursor.lockState = CursorLockMode.None;
        IsActive = false;
    }

    private void Update()
    {
        if (!IsActive) return;

        var ray = new Ray(_camera.position, _camera.forward);
        if (Physics.Raycast(ray, out var hit))
        {
            hit.transform.gameObject.SendMessage("Hover", SendMessageOptions.DontRequireReceiver);
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                hit.transform.gameObject.SendMessage("Click");
            }
        }
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        var t = (MaxAltitude - _focusCoord.Altitude) / (MaxAltitude - (Singleton.Land.SampleHeight(_focusCoord) + MinHeight));
        var translation = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * math.lerp(MaxMovementSpeed, MinMovementSpeed, t);
        var rotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * RotationSpeed;
        _altitude -= Input.mouseScrollDelta.y * math.lerp(MaxZoomSpeed, MinZoomSpeed, t);
        _focusCoord.LocalPlanet += (_focus.localRotation * translation).ToFloat3();
        _focusCoord.Altitude = math.lerp(_focusCoord.Altitude, _altitude, Time.deltaTime * LerpSpeed);
        _focus.Rotate(0,rotation.x,0);

        CameraUtils.SetState(GetTargetState(_camera, _focus, _focusCoord));

        if (_focusCoord.Altitude < Singleton.Land.SampleHeight(_focusCoord) + MinHeight)
        {
            Singleton.PerspectiveController.ZoomIn();
        }
        if (_focusCoord.Altitude > MaxAltitude)
        {
            Singleton.PerspectiveController.ZoomOut();
        }
    }

    private CameraState GetTargetState(Transform camera, Transform focus, Coordinate focusCoord)
    {
        var t = (MaxAltitude - focusCoord.Altitude) / (MaxAltitude - (Singleton.Land.SampleHeight(_focusCoord) + MinHeight));
        var cameraRot = Quaternion.Euler(math.lerp(MaxAngle, MinAngle, t), 0, 0);

        var right = Planet.Transform.InverseTransformDirection(camera.right);
        var up = Planet.Transform.InverseTransformDirection(camera.position.normalized);
        var forward = Quaternion.AngleAxis(90, right) * up;

        return new CameraState(camera, focus)
        {
            CameraParent = focus,
            CameraLocalPosition = Vector3.zero,
            CameraLocalRotation = cameraRot,
            FocusParent = Planet.Transform,
            FocusLocalPosition = focusCoord.LocalPlanet,
            FocusLocalRotation = quaternion.LookRotation(forward, up),
            FieldOfView = math.lerp(MaxFov, MinFov, t * t),
        };
    }
}

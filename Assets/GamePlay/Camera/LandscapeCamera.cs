using Unity.Mathematics;
using UnityEngine;

public class LandscapeCamera : MonoBehaviour
{
    [Header("Altitude")] 
    public float LerpSpeed = 2f;
    public float MaxAltitude = 3000;
    public float MinAltitude = 2000;
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

    private float _altitude;
    private float _targetAltitude;

    public void Enable(Transform camera, Transform focus)
    {
        _camera = camera;
        _focus = focus;
        _focus.parent = FindObjectOfType<Planet>().transform;
        _focus.position = _camera.position;
        _camera.parent = _focus;

        _camera.localPosition = new Vector3(0,0,-1);
        _altitude = _focus.localPosition.magnitude;
        _targetAltitude = _altitude;

        Cursor.lockState = CursorLockMode.Locked;
        IsActive = true;
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
            hit.transform.gameObject.SendMessage("Hover");
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                hit.transform.gameObject.SendMessage("Click");
            }
        }
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        var t = (MaxAltitude - _altitude) / (MaxAltitude - MinAltitude);
        var translation = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * math.lerp(MaxMovementSpeed, MinMovementSpeed, t);
        var rotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * RotationSpeed;

        var right = _camera.right;
        var up = _camera.position.normalized;
        var forward = Quaternion.AngleAxis(90, right) * up;

        _targetAltitude -= Input.mouseScrollDelta.y * math.lerp(MaxZoomSpeed, MinZoomSpeed, t);
        _targetAltitude = math.max(_targetAltitude, MinAltitude);
        _altitude = math.lerp(_altitude, _targetAltitude, Time.deltaTime * LerpSpeed);
        
        t = (MaxAltitude - _altitude) / (MaxAltitude - MinAltitude);

        _focus.LookAt(_focus.position + forward, up);
        _focus.Rotate(0,rotation.x,0);
        _focus.Translate(translation, Space.Self);
        _focus.localPosition = _focus.localPosition.normalized * _altitude;

        _camera.localEulerAngles = new Vector3(1, 0, 0) * math.lerp(MaxAngle, MinAngle, t);
        _camera.GetComponent<Camera>().fieldOfView = math.lerp(MaxFov, MinFov, t * t);

        if (_focus.position.magnitude < MinAltitude)
        {
            Singleton.PerspectiveController.ZoomIn();
        }
        if (_focus.position.magnitude > MaxAltitude)
        {
            Singleton.PerspectiveController.ZoomOut();
        }
    }
}

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
    public bool LockMovement;
    public bool LockRotation;
    public bool LockAltitude;

    public Vector3 FocusPos => _focus.position;
    public float FocusRadius { get; set; }
    public Coordinate FocusCoord { get; private set; }

    public float CameraDistance => _camera.localPosition.magnitude;

    [Space(10)]
    [Range(1,10)]
    public float MovementSpeed = 5f;
    [Range(1,10)]
    public float RotationSpeed = 5f;
    [Range(1,10)]
    public float ZoomSpeed = 1f;

    [Space(10)]
    public PostProcessProfile PostProccessing;

    private float _lerpSpeed = 10f;
    private float _minDistance = 1.5f;
    private float _maxDistance = 1000f;

    private Transform _focus;
    private Transform _camera;

    private float targetCameraDistance = 50;
    private float targetCameraAngle = 0.3f;
    private float targetAltitude = 1000;

    private void Start()
    {
        _focus = transform.parent;
        _camera = transform;
        FocusCoord = new Coordinate(_focus.position);
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }

    private void LateUpdate()
    {        
        Move();
        Rotate();

        PostProccessing.GetSetting<DepthOfField>().focusDistance.value = _camera.localPosition.magnitude;
    }
    
    private void Move()
    {
        var movementMultiplier = MovementSpeed * Time.deltaTime * math.sqrt(_camera.localPosition.magnitude * 2);
        var movementVector = (_focus.right * Input.GetAxis("Horizontal") + _focus.forward * Input.GetAxis("Vertical")) * movementMultiplier;
        var lerpSpeed = Time.deltaTime * _lerpSpeed * 2;

        if (movementVector.magnitude > float.Epsilon && !LockMovement)
        {
            FocusCoord = ClampToTerrain(FocusCoord.xyz + movementVector.ToFloat3());
        }

        _focus.position = ClampToTerrain(Vector3.Lerp(_focus.position, FocusCoord.xyz, lerpSpeed)).xyz;
        var upward = _focus.position.normalized;
        var forward = Vector3.Cross(_focus.right, upward);
        _focus.rotation = Quaternion.LookRotation(forward, upward);
    }

    private void Rotate()
    {
        var maxDir = new Vector3(0, 0.9995f, -0.0005f);
        var minDir = new Vector3(0, 0.0005f, -0.9995f);

        if (!LockRotation)
        {
            if (Input.GetMouseButton(0))
            {
                var horizontalMovement = Input.GetAxis("Mouse X") / Screen.width * RotationSpeed * 550;
                var invertDirectiom = Input.mousePosition.y > (Screen.height / 2) ? -1 : 1;
                _focus.Rotate(new Vector3(0, horizontalMovement * invertDirectiom, 0));

                var verticalMovement = Input.GetAxis("Mouse Y") / Screen.height * RotationSpeed * -1.5f;
                targetCameraAngle = math.clamp(targetCameraAngle + verticalMovement, 0, 1);
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                var depthMovement = 1 - Input.mouseScrollDelta.y * ZoomSpeed * 0.05f;
                targetCameraDistance = math.clamp(targetCameraDistance * depthMovement, _minDistance, _maxDistance);
            }
        }

        var lerpSpeed = Time.deltaTime * _lerpSpeed * 2;
        //minDir = Vector3.Lerp(minDir, maxDir, _distance / _maxDistance);
        var targetLocalPos = Vector3.Lerp(minDir, maxDir, targetCameraAngle) * targetCameraDistance;
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, targetLocalPos, lerpSpeed);
        _camera.position = ClampAboveTerrain(_camera.position).xyz;
        _camera.LookAt(_focus.position, _focus.up);
    }

    private Coordinate ClampAboveTerrain(Coordinate coord)
    {
        var minAltitude = math.max(Singleton.Land.SampleHeight(coord), Singleton.Water.SampleHeight(coord)) + _minDistance;
        coord.Altitude = coord.Altitude < minAltitude ? minAltitude : coord.Altitude;
        return coord;
    }

    private Coordinate ClampToTerrain(Coordinate coord)
    {
        if (!LockAltitude)
        {
            targetAltitude = math.max(Singleton.Land.SampleHeight(coord), Singleton.Water.SampleHeight(coord));
        }
        coord.Altitude = targetAltitude;
        return coord;
    }
}

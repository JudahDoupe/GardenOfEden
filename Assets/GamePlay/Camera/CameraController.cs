using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
    public bool LockMovement;
    public bool LockRotation;
    public bool LockAltitude;
    public bool LockCamera;
    public bool LockFocus;

    public Vector3 FocusPos => Focus.position;
    public float FocusRadius { get; set; }
    public Coordinate FocusCoord { get; private set; }

    public float CameraDistance => Camera.localPosition.magnitude;

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

    public Transform Focus;
    public Transform Camera;

    private float targetCameraDistance = 50;
    private float targetCameraAngle = 0.5f;
    private float targetAltitude = 1000;

    private void Start()
    {
        UnityEngine.Camera.main.depthTextureMode = DepthTextureMode.Depth;

        Focus = transform.parent;
        Camera = transform;
        FocusCoord = new Coordinate(Focus.position);
        targetCameraDistance = Camera.localPosition.magnitude;
    }

    private void LateUpdate()
    {        
        if (!LockMovement) GetMovementInput();
        if (!LockFocus) ApplyMovement();

        if (!LockRotation) GetRotationInput();
        if (!LockCamera) ApplyRotate();

        PostProccessing.GetSetting<DepthOfField>().focusDistance.value = Camera.localPosition.magnitude;
    }

    /**** MOVEMENT ****/
    public void MoveTo(Coordinate coord)
    {
        FocusCoord = ClampAboveTerrain(coord);
    }

    public void Move(float3 v)
    {
        FocusCoord = ClampAboveTerrain(new Coordinate(FocusCoord.xyz + v));
    }

    private void GetMovementInput()
    {
        var movementMultiplier = MovementSpeed * Time.deltaTime * math.sqrt(Camera.localPosition.magnitude * 2);
        var movementVector = (Focus.right * Input.GetAxis("Horizontal") + Focus.forward * Input.GetAxis("Vertical")) * movementMultiplier;

        if (movementVector.magnitude > float.Epsilon)
        {
            FocusCoord = ClampToTerrain(FocusCoord.xyz + movementVector.ToFloat3());
        }
        else
        {
            FocusCoord = ClampToTerrain(FocusCoord.xyz);
        }
    }

    private void ApplyMovement()
    {
        var lerpSpeed = Time.deltaTime * _lerpSpeed * 2;
        Focus.position = ClampAboveTerrain(Vector3.Lerp(Focus.position, FocusCoord.xyz, lerpSpeed)).xyz;
        var upward = Focus.position.normalized;
        var forward = Vector3.Cross(Focus.right, upward);
        Focus.rotation = Quaternion.LookRotation(forward, upward);
    }

    /**** ROTATION ****/

    public void Zoom(float distance)
    {
        targetCameraDistance = distance;
    }

    public void Rotate(Vector2 v)
    {
        var horizontalMovement = v.x * 550;
        Focus.Rotate(new Vector3(0, horizontalMovement, 0));

        var verticalMovement = v.y * -1.5f;
        targetCameraAngle = math.clamp(targetCameraAngle + verticalMovement, 0, 1);
    }

    private void GetRotationInput()
    {
        if (Input.GetMouseButton(0))
        {
            var horizontalMovement = Input.GetAxis("Mouse X") / Screen.width * RotationSpeed * 550;
            var invertDirectiom = Input.mousePosition.y > (Screen.height / 2) ? -1 : 1;
            Focus.Rotate(new Vector3(0, horizontalMovement * invertDirectiom, 0));

            var verticalMovement = Input.GetAxis("Mouse Y") / Screen.height * RotationSpeed * -1.5f;
            targetCameraAngle = math.clamp(targetCameraAngle + verticalMovement, 0, 1);
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            var depthMovement = 1 - Input.mouseScrollDelta.y * ZoomSpeed * 0.05f;
            targetCameraDistance = math.clamp(targetCameraDistance * depthMovement, _minDistance, _maxDistance);
        }
    }

    private void ApplyRotate()
    {
        var maxDir = new Vector3(0, 0.9995f, -0.0005f);
        var minDir = new Vector3(0, 0.0005f, -0.9995f);

        var lerpSpeed = Time.deltaTime * _lerpSpeed * 2;
        var targetLocalPos = Vector3.Lerp(minDir, maxDir, targetCameraAngle) * targetCameraDistance;
        Camera.localPosition = Vector3.Lerp(Camera.localPosition, targetLocalPos, lerpSpeed);
        Camera.position = ClampAboveTerrain(Camera.position).xyz;
        Camera.LookAt(Focus.position, Focus.up);
    }

    /**** HELPERS ****/

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

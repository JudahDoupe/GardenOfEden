using UnityEngine;

public class EditorCameraVisitor : ICameraVisitor
{
    private readonly Plant _editedPlant;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    
    public EditorCameraVisitor(Plant plant, CameraController camera)
    {
        _editedPlant = plant;
        _targetPosition = camera.transform.position;
    }
    
    public void VisitCamera(CameraController camera)
    {
        var t = Time.deltaTime * camera.Speed;
        var bounds = CameraUtils.GetPlantBounds(_editedPlant);
        RotateAroundPlant(t * 10, bounds);
        LookAtPlant(bounds, camera);
        camera.transform.position = Vector3.Lerp(camera.transform.position, _targetPosition, t);
        camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, _targetRotation, t);
    }

    private void RotateAroundPlant(float rotationAngle, Bounds plantBounds)
    {
        var plantPosition = _editedPlant.transform.position;
        var direction = (_targetPosition - plantBounds.center).normalized;
        direction.Scale(new Vector3(1,0,1));
        direction.Normalize();
        var plantSize = Mathf.Max(plantBounds.extents.x, plantBounds.extents.y, plantBounds.extents.z);
        var cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView); // Visible height 1 meter in front
        var distance = 3 * plantSize / cameraView; // Combined wanted distance from the object
        distance += 0.5f * plantSize; // Estimated offset from the center to the outside of the object
        var vector = direction * distance + new Vector3(0, plantBounds.extents.y, 0);
        var newVector = Quaternion.Euler(0, rotationAngle, 0) * vector;
        _targetPosition = plantBounds.center + newVector;
    }

    private void LookAtPlant(Bounds plantBounds, CameraController camera)
    {
        var towardPlantDirection = (plantBounds.center - camera.transform.position).normalized;
        _targetRotation = Quaternion.LookRotation(towardPlantDirection, Vector3.up);
    }
}

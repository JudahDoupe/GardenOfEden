using UnityEngine;

public class EditorCameraVisitor : ICameraVisitor
{
    private readonly Plant _editedPlant;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    
    public EditorCameraVisitor(Plant plant)
    {
        _editedPlant = plant;
        _targetPosition = Camera.main.transform.position;
    }
    
    public void VisitCamera(CameraController camera)
    {
        var t = Time.deltaTime * camera.Speed;
        var bounds = CameraUtils.GetPlantBounds(_editedPlant);
        _targetPosition = CameraUtils.RotateAroundBounds(_targetPosition, bounds,t * 10);
        _targetPosition = CameraUtils.ClampAboveGround(_targetPosition);
        _targetRotation =  CameraUtils.LookAtBoundsCenter(bounds);
        camera.transform.position = Vector3.Lerp(camera.transform.position, _targetPosition, t);
        camera.transform.position = CameraUtils.ClampAboveGround(camera.transform.position);
        camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, _targetRotation, t);
    }
}

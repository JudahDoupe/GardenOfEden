using UnityEngine;

public class EditorVisitor : ICameraVisitor
{
    private Plant _editedPlant;
    private Vector3 _targetPosition;
    
    public EditorVisitor(Plant plant, CameraController camera)
    {
        _editedPlant = plant;
        _targetPosition = camera.transform.position;
    }
    
    public void VisitCamera(CameraController camera)
    {
        var distance = Time.deltaTime * camera.Speed;
        RotateAroundPlant(distance);
        camera.transform.position = Vector3.Lerp(camera.transform.position, _targetPosition, distance);
    }

    private void RotateAroundPlant(float rotationAngle)
    {
        var plantPosition = _editedPlant.transform.position;
        var direction = (_targetPosition - plantPosition).normalized;
        var bounds = GetBoundsRecursive( new Bounds(plantPosition, new Vector3(1, 1, 1)), _editedPlant);
        var distance = bounds.extents.magnitude;
        var vector = direction * distance;
        var newVector = Quaternion.Euler(0, rotationAngle, 0) * vector;
        _targetPosition = plantPosition + newVector;
    }
}

using UnityEngine;

public class CameraTransform : MonoBehaviour
{
    public float MoveSpeed = 0.25f;
    public float LookSpeed = 0.75f;
    public Vector3 TargetPosition;
    public Vector3 TargetFocusPosition;

    void LateUpdate()
    {
        LerpTowardTargets();
    }

    private void LerpTowardTargets()
    {
        var targetPosition = Vector3.Lerp(transform.position, TargetPosition, MoveSpeed * Time.deltaTime);
        targetPosition.y = Mathf.Max(targetPosition.y, DI.LandService.SampleTerrainHeight(targetPosition) + 0.5f);
        transform.position = targetPosition;

        var targetRotation = Quaternion.LookRotation(TargetFocusPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, LookSpeed * Time.deltaTime);
    }
}

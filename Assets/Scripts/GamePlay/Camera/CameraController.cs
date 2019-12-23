using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MoveSpeed;
    public float LookSpeed;

    public void MoveTo(Vector3 position)
    {
        _targetPosition = position;
        _rotateAroundPoint = null;
        _rotateAroundOffset = null;
        SetPlayerEnabled(false);
    }

    public void LookAt(Vector3 position)
    {
        _targetLookPosition = position;
    }

    public void RotateAround(Vector3 position, Vector3 offset)
    {
        _rotateAroundPoint = position;
        _rotateAroundOffset = offset;
        _targetLookPosition = position;
        SetPlayerEnabled(false);
    }

    public void EnablePlayerControlls()
    {
        StartCoroutine(ResetCameraToHead());
    }
    public void DisablePlayerControlls()
    {
        SetPlayerEnabled(false);
    }

    /* INNER MECHINATIONS */

    private Vector3 _targetPosition;
    private Vector3 _targetLookPosition;

    private Vector3? _rotateAroundPoint;
    private Vector3? _rotateAroundOffset;

    private FirstPersonController _player;
    private bool IsInUse;
    private float currentAngle = 0;
    private void Start()
    {
        _player = GetComponentInParent<FirstPersonController>();
    }
    private void Update()
    {
        if (!IsInUse) return;

        if (_rotateAroundPoint.HasValue)
        {
            currentAngle = (currentAngle + (Time.deltaTime * MoveSpeed * _rotateAroundOffset.Value.z * 0.1f * Mathf.PI)) % 360;
            var offset = Quaternion.AngleAxis(currentAngle, Vector3.up) * _rotateAroundOffset;
            _targetPosition = _rotateAroundPoint.Value + offset.Value;
        }

        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime);

        var targetRotation = Quaternion.LookRotation(_targetLookPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, LookSpeed * Time.deltaTime);
    }

    private void SetPlayerEnabled(bool enabled)
    {
        _player.IsCameraMovable = enabled;
        _player.IsPlayerMovable = enabled;
        _player.IsFocusEnabled = enabled;
        _player.IsMouseHidden = enabled;
        IsInUse = !enabled;
    }

    private IEnumerator ResetCameraToHead()
    {
        IsInUse = false;
        while (Vector3.Distance(transform.localPosition, Vector3.zero) > 0.05f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.unscaledDeltaTime * 5);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.unscaledDeltaTime * 5);
            yield return new WaitForEndOfFrame();
        }

        transform.localPosition = Vector3.zero;
        SetPlayerEnabled(true);
    }
}

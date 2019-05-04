using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantCreationPedistal : Interactable
{
    private FirstPersonController _creator;
    private Vector3 _cameraOffset = new Vector3(0, 0.5f, -2);

    public Transform _cameraTarget;
    public Vector3 CameraMoveSpeed = new Vector3(1,0.1f,0.5f);

    public override void Interact(FirstPersonController player)
    {
        _creator = player;
        StartCreation();
    }
    public override Vector3 InteractionPosition()
    {
        return transform.Find("Soil").transform.position;
    }

    public void StartCreation()
    {
        _creator.IsCameraMovable = false;
        _creator.IsPlayerMovable = false;
        _creator.IsMouseHidden = false;
        _creator.IsFocusEnabled = false;

        _creator.Camera.transform.parent = _cameraTarget;
    }
    public void EndCreation()
    {
        _creator.IsCameraMovable = true;
        _creator.IsPlayerMovable = true;
        _creator.IsMouseHidden = true;
        _creator.IsFocusEnabled = true;
        _creator = null;

        _creator.Camera.transform.parent = _creator.transform.Find("Head");
        _creator.Camera.transform.position = _creator.transform.Find("HeadModel").position;
    }

    void LateUpdate()
    {
        if (_creator != null)
        {
            UpdateCamera();
        }
    }
    public void UpdateCamera()
    {
        _cameraTarget.Translate(0, Input.GetAxis("Vertical") * CameraMoveSpeed.y, 0);
        _cameraTarget.position = _cameraTarget.position.Clamp(new Vector3(0, InteractionPosition().y, 0), new Vector3(0, InteractionPosition().y + 3, 0));

        _cameraOffset += new Vector3(0, 0, Input.mouseScrollDelta.y * CameraMoveSpeed.z);
        _cameraOffset = _cameraOffset.Clamp(new Vector3(0,0,-5), new Vector3(0,5,-1));
        _creator.Camera.transform.localPosition = Vector3.Lerp(_creator.Camera.transform.localPosition,
                                                              _cameraOffset, 
                                                              Time.unscaledDeltaTime * 5);

        _cameraTarget.Rotate(0, Input.GetAxis("Horizontal") * CameraMoveSpeed.x, 0);
        _creator.Camera.transform.LookAt(_cameraTarget);
    }
}

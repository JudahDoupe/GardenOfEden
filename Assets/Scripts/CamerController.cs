using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerController : MonoBehaviour
{

    public Transform Center;
    public float CameraSpeed = 1;

    private float _height = 4;
    private float _setback = 10;

    void LateUpdate()
    {
        var verticalMultiplier = 0;
        var horizontalMultiplier = 0;
        if (Input.GetKey(KeyCode.W)) verticalMultiplier++;
        if (Input.GetKey(KeyCode.A)) horizontalMultiplier--;
        if (Input.GetKey(KeyCode.S)) verticalMultiplier--;
        if (Input.GetKey(KeyCode.D)) horizontalMultiplier++;

        _height += CameraSpeed * verticalMultiplier;
        Camera.main.transform.position = Center.position;
        Camera.main.transform.Rotate(Vector3.up, CameraSpeed * 36 * -horizontalMultiplier);
        Camera.main.transform.Translate(new Vector3(0, _height, -_setback));
    }
}

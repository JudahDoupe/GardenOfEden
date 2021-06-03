using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(Camera.main.transform.position, Camera.main.transform.up);
    }
}

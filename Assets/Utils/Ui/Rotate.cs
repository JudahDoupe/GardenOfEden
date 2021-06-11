using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 Speed;
    public Space Space;

    void Update()
    {
        transform.Rotate(Speed * Time.deltaTime, Space);
    }
}

using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float Speed = 1;
    
    public ICameraVisitor CameraVisitor { get; set;  }

    public void Accept(ICameraVisitor visitor)
    {
        visitor.VisitCamera(this);
    }

    private void Start()
    {
        CameraVisitor = new EcosystemCameraVisitor(FindObjectOfType<Plant>());
    }

    private void LateUpdate()
    {
        CameraVisitor?.VisitCamera(this);
    }
}

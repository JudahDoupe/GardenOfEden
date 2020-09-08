using UnityEngine;

public class CameraController : MonoBehaviour
{    
    public ICameraVisitor CameraVisitor { get; set;  }

    public void Accept(ICameraVisitor visitor)
    {
        visitor.VisitCamera(this);
    }

    private void Start()
    {
        CameraVisitor = new EditorCameraVisitor(FindObjectOfType<Plant>());
    }

    private void LateUpdate()
    {
        CameraVisitor?.VisitCamera(this);
    }
}

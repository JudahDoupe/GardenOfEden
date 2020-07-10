using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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
        CameraVisitor = new SpeciesCameraVisitor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CameraVisitor = new EditorCameraVisitor(GameObject.FindObjectOfType<Plant>());
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CameraVisitor = new SpeciesCameraVisitor();
        }
    }
    
    private void LateUpdate()
    {
        CameraVisitor?.VisitCamera(this);
    }
}

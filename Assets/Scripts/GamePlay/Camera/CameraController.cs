using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
    public float Speed = 1;
    
    public ICameraVisitor TransformVisitor { get; set;  }
    public PostProcessProfile PPProfile { get; private set; }

    public void Accept(ICameraVisitor visitor)
    {
        visitor.VisitCamera(this);
    }

    private void Start()
    {
        PPProfile = transform.GetComponent<PostProcessProfile>();
    }
    
    private void LateUpdate()
    {
        TransformVisitor.VisitCamera(this);
    }
}

using UnityEngine;

public class Internode : MonoBehaviour
{
    public Plant Plant { get; set; }
    public RenderingInstanceData Mesh { get; set; }

    public Node Head { get; set; }
    public Node Base { get; set; }

    public float Length { get; set; }
    public float Radius { get; set; }

    public static Internode Create(Node headNode, Node baseNode)
    {
        var internode = baseNode.gameObject.AddComponent<Internode>();

        internode.Mesh = InstancedMeshRenderer.AddInstance("Stem");
        internode.Head = headNode;
        internode.Base = baseNode;

        return internode;
    }

    public virtual void UpdateMesh()
    {
        Head.transform.localPosition = Base.transform.localRotation * Vector3.forward * Length;
        Mesh.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(Radius, Radius, Length));
    }

    public void Kill()
    {
        InstancedMeshRenderer.RemoveInstance(Mesh);
    }
}
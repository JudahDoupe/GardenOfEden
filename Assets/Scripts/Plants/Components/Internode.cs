using UnityEngine;

public class Internode : MonoBehaviour
{
    public Plant Plant { get; set; }
    public RenderingInstanceData Mesh { get; set; }

    public Node Head { get; set; }
    public Node Base { get; set; }

    public PlantDna.Internode Dna { get; set; }

    public float Length;
    public float Radius;

    public static Internode Create(Node headNode, Node baseNode)
    {
        var internode = headNode.gameObject.AddComponent<Internode>();

        internode.Mesh = InstancedMeshRenderer.AddInstance("Stem");
        internode.Head = headNode;
        internode.Base = baseNode;
        internode.Dna = baseNode.Plant.Dna.GetInternodeDna(headNode.Type);

        return internode;
    }

    public virtual void UpdateMesh()
    {
        Head.transform.position = Base.transform.forward * Length + Base.transform.position;
        Mesh.Matrix = Matrix4x4.TRS(Head.transform.position, 
                                    Quaternion.LookRotation(Head.transform.position - Base.transform.position),
                                    new Vector3(Radius, Radius, Length));
    }

    public void Kill()
    {
        InstancedMeshRenderer.RemoveInstance(Mesh);
    }
}
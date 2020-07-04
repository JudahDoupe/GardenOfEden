using System.Collections;
using UnityEngine;

public class Internode : MonoBehaviour
{
    public Plant Plant { get; set; }
    public RenderingInstanceData Mesh { get; set; }

    public Node Head { get; set; }
    public Node Base { get; set; }

    public PlantDna.InternodeDna Dna { get; set; }

    public float Length;
    public float Radius;

    public static Internode Create(Node headNode, Node baseNode)
    {
        var internode = headNode.gameObject.AddComponent<Internode>();

        internode.Mesh = InstancedMeshRenderer.AddInstance("Stem");
        internode.Head = headNode;
        internode.Base = baseNode;
        internode.Dna = headNode.Dna.InternodeDna;

        return internode;
    }

    public void Kill()
    {
        InstancedMeshRenderer.RemoveInstance(Mesh);
    }
}
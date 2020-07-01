using System.Collections;
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
        internode.Dna = headNode.Dna.Internode;

        return internode;
    }

    public virtual void UpdateMesh()
    {
        Head.transform.position = Head.transform.forward * Length + Base.transform.position;
        var vector = Head.transform.position - Base.transform.position;
        var rotation = vector == Vector3.zero ? Head.transform.rotation : Quaternion.LookRotation(vector);
        Mesh.Matrix = Matrix4x4.TRS(Head.transform.position,
                                    rotation,
                                    new Vector3(Radius, Radius, Length));
    }
    public IEnumerator SmoothUpdateMesh(float seconds)
    {
        if (Mesh == null) yield break;

        var oldPosition = Mesh.Matrix.GetColumn(3);
        var oldRotation = Quaternion.LookRotation(
            Mesh.Matrix.GetColumn(2),
            Mesh.Matrix.GetColumn(1)
        );
        var oldScale = new Vector3(
            Mesh.Matrix.GetColumn(0).magnitude,
            Mesh.Matrix.GetColumn(1).magnitude,
            Mesh.Matrix.GetColumn(2).magnitude
        );

        var vector = Head.transform.position - Base.transform.position;
        var newRotation = vector == Vector3.zero ? Head.transform.rotation : Quaternion.LookRotation(vector);
        var newScale = new Vector3(Radius, Radius, Length);

        var t = 0f;
        while (t < seconds)
        {
            var position = Head.transform.forward * Length + Base.transform.position;
            Head.transform.position = Vector3.Lerp(oldPosition, position, t / seconds);
            Mesh.Matrix = Matrix4x4.TRS(Vector3.Lerp(oldPosition, position, t / seconds),
                                        Quaternion.Lerp(oldRotation, newRotation, t / seconds),
                                        Vector3.Lerp(oldScale, newScale, t / seconds));
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }

        var newPosition = Head.transform.forward * Length + Base.transform.position;
        Head.transform.position = newPosition;
        Mesh.Matrix = Matrix4x4.TRS(newPosition,
                                    newRotation,
                                    newScale);
    }

    public void Kill()
    {
        InstancedMeshRenderer.RemoveInstance(Mesh);
    }
}
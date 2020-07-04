using System.Collections;
using UnityEngine;

public class VisualGrowthVisitor : IVisitor
{
    public float SpeedInSeconds { get; set; }
    public VisualGrowthVisitor(float speedInSeconds)
    {
        SpeedInSeconds = speedInSeconds;
    }

    public void VisitPlant(Plant plant)
    {
        if (SpeedInSeconds > Time.deltaTime)
        {
            plant.StartCoroutine(StartGrowTimer(plant));
        }
        UpdateMeshRecursively(plant);
    }

    private void UpdateMeshRecursively(Node node)
    {
        foreach (var branchNode in node.Branches)
        {
            UpdateMeshRecursively(branchNode);
        }
        if (SpeedInSeconds > Time.deltaTime)
        {
            node.StartCoroutine(SmoothUpdateMesh(node, SpeedInSeconds));
        }
        else
        {
            UpdateMesh(node);
        }
    }
    private IEnumerator StartGrowTimer(Plant plant)
    {
        plant.IsGrowing = true;
        yield return new WaitForSeconds(SpeedInSeconds);
        plant.IsGrowing = false;
    }

    private void UpdateMesh(Node node)
    {
        if (node.Internode != null)
        {
            node.transform.position = node.transform.forward * node.Internode.Length + node.Base.transform.position;
            var vector = node.transform.position - node.Base.transform.position;

            node.Internode.Mesh.Position = node.transform.position;
            node.Internode.Mesh.Rotation = vector == Vector3.zero ? node.transform.rotation : Quaternion.LookRotation(vector);
            node.Internode.Mesh.Scale = new Vector3(node.Internode.Radius, node.Internode.Radius, node.Internode.Length);
            node.Internode.Mesh.UpdateMatrix();
        }
        if (node.Mesh != null)
        {
            node.Mesh.Position = node.transform.position;
            node.Mesh.Rotation = node.transform.rotation;
            node.Mesh.Scale = new Vector3(node.Size, node.Size, node.Size);
            node.Mesh.UpdateMatrix();
        }
    }

    private IEnumerator SmoothUpdateMesh(Node node, float seconds)
    {
        var oldPosition = node.transform.position;
        var oldRotation = node.transform.rotation;
        var oldScale = new Vector3(node.Size, node.Size, node.Size);

        var oldIndernodeRotation = Quaternion.identity;
        var oldInternodeScale = new Vector3(0, 0, 0);

        if (node.Internode != null)
        {
            oldIndernodeRotation = node.Internode.Mesh.Rotation;
            oldInternodeScale = node.Internode.Mesh.Scale;

            node.transform.position = node.transform.forward * node.Internode.Length + node.Base.transform.position;
            var vector = node.transform.position - node.Base.transform.position;
            node.Internode.Mesh.Position = node.transform.position;
            node.Internode.Mesh.Rotation = vector == Vector3.zero ? node.transform.rotation : Quaternion.LookRotation(vector);
            node.Internode.Mesh.Scale = new Vector3(node.Internode.Radius, node.Internode.Radius, node.Internode.Length);
        }
        if (node.Mesh != null)
        {
            oldPosition = node.Mesh.Position;
            oldRotation = node.Mesh.Rotation;
            oldScale = node.Mesh.Scale;

            node.Mesh.Position = node.transform.position;
            node.Mesh.Rotation = node.transform.rotation;
            node.Mesh.Scale = new Vector3(node.Size, node.Size, node.Size);
        }

        var t = 0f;
        while (t < seconds)
        {
            if (node.Internode != null)
            {
                node.Internode.Mesh.Matrix = Matrix4x4.TRS(Vector3.Lerp(oldPosition, node.Internode.Mesh.Position, t / seconds),
                                                           Quaternion.Lerp(oldIndernodeRotation, node.Internode.Mesh.Rotation, t / seconds),
                                                           Vector3.Lerp(oldInternodeScale, node.Internode.Mesh.Scale, t / seconds));
            }
            if (node.Mesh != null)
            {
                node.Mesh.Matrix = Matrix4x4.TRS(Vector3.Lerp(oldPosition, node.Mesh.Position, t / seconds),
                                                 Quaternion.Lerp(oldRotation, node.Mesh.Rotation, t / seconds),
                                                 Vector3.Lerp(oldScale, node.Mesh.Scale, t / seconds));
            }
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }

        if (node.Internode != null)
        {
            node.Internode.Mesh.UpdateMatrix();
        }
        if (node.Mesh != null)
        {
            node.Mesh.UpdateMatrix();
        }
    }
}
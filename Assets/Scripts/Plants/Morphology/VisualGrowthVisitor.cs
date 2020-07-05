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
        UpdateMeshValues(node);
        node.InternodeMesh?.UpdateMatrix();
        node.NodeMesh?.UpdateMatrix();
    }

    private IEnumerator SmoothUpdateMesh(Node node, float seconds)
    {
        var oldPosition = node.NodeMesh?.Position ?? node.transform.position;
        var oldRotation = node.NodeMesh?.Rotation ?? node.transform.rotation;
        var oldScale = node.NodeMesh?.Scale ?? new Vector3(node.Size, node.Size, node.Size);

        var oldIndernodePosition = node.InternodeMesh?.Position ?? oldPosition;
        var oldIndernodeRotation = node.InternodeMesh?.Rotation ?? oldRotation;
        var oldInternodeScale = node.InternodeMesh?.Scale ?? new Vector3(0, 0, 0);

        UpdateMeshValues(node);

        var delta = 0f;
        while (delta < seconds)
        {
            var t = delta / seconds;
            if (node.InternodeMesh != null)
            {
                node.InternodeMesh.Matrix = Matrix4x4.TRS(Vector3.Lerp(oldIndernodePosition, node.InternodeMesh.Position, t),
                                                           Quaternion.Lerp(oldIndernodeRotation, node.InternodeMesh.Rotation, t),
                                                           Vector3.Lerp(oldInternodeScale, node.InternodeMesh.Scale, t));
            }
            if (node.NodeMesh != null)
            {
                node.NodeMesh.Matrix = Matrix4x4.TRS(Vector3.Lerp(oldPosition, node.NodeMesh.Position, t),
                                                 Quaternion.Lerp(oldRotation, node.NodeMesh.Rotation, t),
                                                 Vector3.Lerp(oldScale, node.NodeMesh.Scale, t));
            }
            yield return new WaitForEndOfFrame();
            delta += Time.deltaTime;
        }
    }

    private void UpdateMeshValues(Node node)
    {
        if (node.InternodeMesh != null)
        {
            node.transform.position = node.transform.forward * node.InternodeLength + node.Base.transform.position;
            var vector = node.transform.position - node.Base.transform.position;
            node.InternodeMesh.Position = node.transform.position;
            node.InternodeMesh.Rotation = vector == Vector3.zero ? node.transform.rotation : Quaternion.LookRotation(vector);
            node.InternodeMesh.Scale = new Vector3(node.InternodeRadius, node.InternodeRadius, node.InternodeLength);
        }
        if (node.NodeMesh != null)
        {
            node.NodeMesh.Position = node.transform.position;
            node.NodeMesh.Rotation = node.transform.rotation;
            node.NodeMesh.Scale = new Vector3(node.Size, node.Size, node.Size);
        }
    }
}
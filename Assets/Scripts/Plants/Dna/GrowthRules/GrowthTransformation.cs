using UnityEngine;

public static class GrowthTransformations
{
    public static Node Grow(this Node node, float rate)
    {
        node.Size = CalculateGrowth(node.Dna.Size, node.Size, rate);
        node.InternodeLength = CalculateGrowth(node.Dna.InternodeLength, node.InternodeLength, rate);
        node.InternodeRadius = CalculateGrowth(node.Dna.InternodeRadius, node.InternodeRadius, rate);
        return node;
    }
    public static Node Level(this Node node, float rate)
    {
        var v = node.transform.rotation.eulerAngles;
        var flat = Quaternion.Euler(0, v.y, v.z);
        node.transform.rotation = Quaternion.Slerp(node.transform.rotation, flat, rate);
        return node;
    }
    public static Node AddNode(this Node node, string type)
    {
        return node.Base.AddNodeAfter(type);
    }
    public static Node AddNodeAfter(this Node node, string type)
    {
        var newNode = new GameObject("Node").AddComponent<Node>();
        
        node.Branches.Add(newNode);

        newNode.CreationDate = Singleton.TimeService.Day;
        newNode.Plant = node.Plant;
        newNode.Base = node;
        newNode.transform.parent = node.transform;
        newNode.transform.localPosition = new Vector3(0, 0, 0);
        newNode.transform.localRotation = Quaternion.identity;
        newNode.SetType(type);

        return newNode;
    }
    public static Node AddNodeBefore(this Node node, string type)
    {
        var baseNode = node.Base;
        var middleNode = new GameObject(type).AddComponent<Node>();

        baseNode.Branches.Remove(node);
        baseNode.Branches.Add(middleNode);

        middleNode.CreationDate = Singleton.TimeService.Day;
        middleNode.Plant = node.Plant;
        middleNode.Base = baseNode;
        middleNode.Branches.Add(node);
        middleNode.transform.parent = baseNode.transform;
        middleNode.transform.position = node.transform.position;
        middleNode.transform.rotation = node.transform.rotation;
        middleNode.InternodeLength = node.InternodeLength;
        middleNode.InternodeRadius = node.InternodeRadius;
        middleNode.InternodeMesh = node.InternodeMesh ?? InstancedMeshRenderer.AddInstance("Stem");
        middleNode.Type = type;

        node.Base = middleNode;
        node.transform.parent = middleNode.transform;
        node.transform.localPosition = new Vector3(0, 0, 0);
        node.transform.localRotation = Quaternion.identity;
        node.InternodeLength = 0; 
        node.InternodeRadius = 0;

        node.SetType(node.Type);

        return middleNode;
    }
    public static Node SetType(this Node node, string type)
    {
        node.Type = type;
        node.gameObject.name = type;
        if (node.NodeMesh != null)
        {
            InstancedMeshRenderer.RemoveInstance(node.NodeMesh);
            node.NodeMesh = null;
        }
        if (node.InternodeMesh != null)
        {
            InstancedMeshRenderer.RemoveInstance(node.InternodeMesh);
            node.InternodeMesh = null;
        }
        if (!string.IsNullOrWhiteSpace(node.Dna.MeshId))
        {
            node.NodeMesh = InstancedMeshRenderer.AddInstance(node.Dna.MeshId);
        }
        if (node.Dna.InternodeLength > 0.001)
        {
            node.InternodeMesh = InstancedMeshRenderer.AddInstance("Stem");
        }
        return node;
    }
    public static void Kill(this Node node)
    {
        foreach (var branch in node.Branches.ToArray())
        {
            branch.Kill();
        }

        if (node.Base != null) node.Base.Branches.Remove(node);
        if (node.NodeMesh != null) InstancedMeshRenderer.RemoveInstance(node.NodeMesh);
        if (node.InternodeMesh != null) InstancedMeshRenderer.RemoveInstance(node.InternodeMesh);

        UnityEngine.Object.Destroy(node.gameObject);
    }
    public static Node Roll(this Node node, float degrees)
    {
        degrees += Random.Range(-10, 10);
        node.transform.Rotate(new Vector3(0, 0, degrees), Space.Self);
        return node; 
    }
    public static Node Pitch(this Node node, float degrees)
    {
        degrees += Random.Range(-10, 10);
        node.transform.Rotate(new Vector3(degrees, 0, 0), Space.Self);
        return node;
    }
    public static Node Yaw(this Node node, float degrees)
    {
        degrees += Random.Range(-10, 10);
        node.transform.Rotate(new Vector3(0, degrees, 0), Space.Self);
        return node;
    }


    private static float CalculateGrowth(float max, float current, float rate)
    {
        return Mathf.Min(max, current + (rate * max));
    }
}
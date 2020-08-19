using UnityEngine;

public static class GrowthTransformations
{
    public static Node Grow(this Node node, float rate)
    {
        node.Size = CalculateGrowth(node.Dna.Size, node.Size, rate);
        node.InternodeLength = CalculateGrowth(node.Dna.InternodeLength, node.InternodeLength, rate);
        node.InternodeRadius = CalculateGrowth(node.Dna.InternodeRadius, node.InternodeRadius, rate);
        var angle = 1 - Mathf.Abs(Vector3.Dot(node.transform.forward, Vector3.up));
        node.SurfaceArea = ((node.InternodeLength * node.InternodeRadius) + (node.Size * node.Size)) * angle;
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

        PlantMessageBus.NewNode.Publish(newNode);
        return newNode;
    }
    public static Node AddNodeBefore(this Node node, string type)
    {
        var baseNode = node.Base;
        Node middleNode;
        switch (type)
        {
            case NodeType.Plant:
                middleNode = new GameObject(type).AddComponent<Plant>();
                break;
            default:
                middleNode = new GameObject(type).AddComponent<Node>();
                break;
        }

        if (baseNode != null)
        {
            baseNode.Branches.Remove(node);
            baseNode.Branches.Add(middleNode);
            middleNode.transform.parent = baseNode.transform;
        }

        middleNode.CreationDate = Singleton.TimeService.Day;
        middleNode.Plant = node.Plant;
        middleNode.Base = baseNode;
        middleNode.Branches.Add(node);
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

        PlantMessageBus.NewNode.Publish(middleNode);
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
        node.Plant = null;

        PlantMessageBus.NodeDeath.Publish(node);
        if (node is Plant plant)
        {
            PlantMessageBus.PlantDeath.Publish(plant);
        }

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
    public static Node Jitter(this Node node, float degrees)
    {
        var angle = Random.Range(-degrees, degrees);
        node.transform.Rotate(new Vector3(angle, angle, angle), Space.Self);
        return node;
    }
    public static Plant Seperate(this Node node)
    {
        node.Base.Branches.Remove(node);
        node.Base = null;
        node.transform.parent = null;
        var plant = node.AddNodeBefore(NodeType.Plant) as Plant;
        plant.PlantDna = node.Plant.PlantDna.CopyDna();
        node.Plant = plant;

        var height = plant.transform.position.y - Singleton.LandService.SampleTerrainHeight(plant.transform.position);
        var distance = Mathf.Max(height, 3);
        var newPos = plant.transform.position + new Vector3(Random.Range(-distance, distance), 0, Random.Range(-distance, distance));
        newPos.y = Singleton.LandService.SampleTerrainHeight(newPos);
        plant.transform.position = newPos;
        plant.transform.localEulerAngles = new Vector3(-90, Random.Range(0, 365), 0);
        
        return plant;
    }


    private static float CalculateGrowth(float max, float current, float rate)
    {
        return Mathf.Min(max, current + (rate * max));
    }
}
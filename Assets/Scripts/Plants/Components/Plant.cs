using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public float lastUpdateDate;
    public int PlantId;
    public PlantDna Dna;

    public List<IGrowthRule> GrowthRules = new List<IGrowthRule>()
    {
        new CompositeGrowthRule()
            .WithCondition(x => x.Type == NodeType.ApicalBud)
            .WithCondition(x => x.IsPlantOlder(13))
            .WithModification(x => x.Type = NodeType.Node)
            .WithModification(x => Node.Create(NodeType.Flower, x)),        
        new CompositeGrowthRule()
            .WithCondition(x => x.Type == NodeType.ApicalBud)
            .WithModification(x => x.Type = NodeType.Node)
            .WithModification(x => Node.Create(NodeType.ApicalBud, x).Roll(Constants.FibonacciDegrees))
            .WithModification(x => Node.Create(NodeType.Leaf, x).Pitch(10).Roll(-90)),
        new CompositeGrowthRule()
            .WithCondition(x => x.Type == NodeType.Leaf)
            .WithModification(x => x.Level(0.3f)),
        new CompositeGrowthRule()
            .WithCondition(x => x.Type == NodeType.Leaf)
            .WithCondition(x => x.IsLevel())
            .WithModification(x => x.Kill()),
        new CompositeGrowthRule()
            .WithModification(x => x.Grow()),
        new CompositeGrowthRule()
            .WithCondition(x => x.Internode != null)
            .WithModification(x => x.GrowInternode()),
    };

    public Node Shoot { get; set; }
    public Root Root { get; set; }

    public bool IsAlive { get; set; } = true;

    public Volume WaterCapacity = Volume.FromCubicMeters(5);
    public Volume StoredWater { get; set; }
    public Area StoredLight { get; set; }

    void Start()
    {
        Shoot = Node.Create(NodeType.ApicalBud, null, this);
        Root = Root.Create(this);
        lastUpdateDate = EnvironmentApi.GetDate();

        DI.LightService.AddLightAbsorber(this, (absorbedLight) => StoredLight += absorbedLight);
        DI.PlantGrowthService.AddPlant(this);
    }

    public void UpdateMesh()
    {
        UpdateMeshRecursively(Shoot);
    }
    private void UpdateMeshRecursively(Node node)
    {
        foreach(var branchNode in node.Branches)
        {
            UpdateMeshRecursively(branchNode);
        }
        node.UpdateMesh();
    }

    public void Kill()
    {
        IsAlive = false;
        Shoot.Kill();
        Destroy(gameObject);
    }

    public void Accept(IVisitor Visitor)
    {
        Visitor.VisitPlant(this);
    }
}
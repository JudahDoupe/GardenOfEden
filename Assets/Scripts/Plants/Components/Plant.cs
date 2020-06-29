using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public float lastUpdateDate;
    public int PlantId;
    public PlantDna Dna;

    public List<IGrowthRule> GrowthRules;

    public Node Shoot { get; set; }
    public Root Root { get; set; }

    public bool IsAlive { get; set; } = true;

    public Volume WaterCapacity = Volume.FromCubicMeters(5);
    public Volume StoredWater { get; set; }
    public Area StoredLight { get; set; }

    void Start()
    {
        Shoot = Node.Create(PlantDna.NodeType.ApicalBud, null, this);
        Root = Root.Create(this);
        lastUpdateDate = EnvironmentApi.GetDate();
        GrowthRules = Dna.GetGrowthRules().ToList();

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
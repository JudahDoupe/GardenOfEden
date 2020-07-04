using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public float lastUpdateDate;
    public int PlantId;
    public PlantDna Dna;

    public List<IGrowthRule> GrowthRules { get; set; }

    public Node Shoot { get; set; }
    public Root Root { get; set; }

    public bool IsAlive { get; set; } = true;
    public bool IsGrowing { get; set; } = false;

    public Volume WaterCapacity = Volume.FromCubicMeters(5);
    public Volume StoredWater { get; set; }
    public Area StoredLight { get; set; }

    void Start()
    {
        Shoot = Node.Create(null, this);
        Shoot.SetType("ApicalBud");
        Root = Root.Create(this);
        lastUpdateDate = EnvironmentApi.GetDate();
        GrowthRules = Dna.GetGrowthRules().ToList();

        DI.LightService.AddLightAbsorber(this, (absorbedLight) => StoredLight += absorbedLight);
        DI.GrowthService.AddPlant(this);
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
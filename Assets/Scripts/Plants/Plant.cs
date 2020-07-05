using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plant : Node
{
    public float lastUpdateDate;
    public int PlantId;
    public PlantDna PlantDna;

    public Root Root { get; set; }

    public bool IsAlive { get; set; } = true;
    public bool IsGrowing { get; set; } = false;

    public Volume WaterCapacity = Volume.FromCubicMeters(5);
    public Volume StoredWater { get; set; }
    public Area StoredLight { get; set; }

    void Start()
    {
        CreationDate = EnvironmentApi.GetDate();
        lastUpdateDate = CreationDate;
        Plant = this;
        Type = "Plant";
        
        foreach (var node in PlantDna.Nodes)
        {
            node.Update();
        }

        this.AddNodeAfter("Bud",0,0,0);
        Root = Root.Create(this);

        //DI.LightService.AddLightAbsorber(this, (absorbedLight) => StoredLight += absorbedLight);
        DI.GrowthService.AddPlant(this);
    }

    public void Accept(IVisitor Visitor)
    {
        Visitor.VisitPlant(this);
    }
}
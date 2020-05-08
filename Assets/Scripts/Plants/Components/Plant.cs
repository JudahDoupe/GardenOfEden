using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plant : TimeTracker
{
    public int PlantId;
    public PlantDna Dna;

    public Node Shoot { get; set; }
    public Root Root { get; set; }

    public bool IsAlive = true;
    public bool IsGrowing = false;

    public int TotalNodes => transform.GetComponentsInChildren<Node>()?.Length ?? 1;

    public Area StoredLight { get; set; } //TODO: store light in a more useful unit
    public bool HasLightBeenAbsorbed { get; set; }

    public Volume WaterCapacity = Volume.FromCubicMeters(5);
    public Volume StoredWater;
    public bool HasWaterBeenAbsorbed { get; set; }

    public Volume StoredStarch;

    void Start()
    {
        CreationDate = EnvironmentApi.GetDate();
        LastUpdateDate = CreationDate;

        Shoot = Node.Create(this, null, CreationDate);
        Root = Root.Create(this);

        DI.LightService.AddLightAbsorber(this, 
            (absorbedLight) => {
                HasLightBeenAbsorbed = true;
                StoredLight += absorbedLight;
                TryPhotosynthesis();
            });

        DI.GrowthService.StartPlantGrowth(this);
    }

    public void Grow()
    {
        LastUpdateDate = EnvironmentApi.GetDate();
        StoredStarch = Shoot.Grow(StoredStarch);
    }

    public void Kill()
    {
        IsAlive = false;
        Shoot.Kill();
        DI.GrowthService.StopPlantGrowth(this);
        Destroy(gameObject);
    }

    public void TryPhotosynthesis()
    {
        if (HasLightBeenAbsorbed && HasWaterBeenAbsorbed)
        {
            //if (StoredStarch < StarchCapacity) TODO: this value needs to be scaled with time
            //{
                Volume producedStarch;
                if(StoredLight * 1 < StoredWater)
                {
                    producedStarch = StoredLight * 1;
                }
                else
                {
                    producedStarch = StoredWater;
                }

                StoredStarch += producedStarch;
                StoredWater -= producedStarch;
           // }

            StoredLight = Area.FromSquareMeters(0);
            HasWaterBeenAbsorbed = false;
            HasLightBeenAbsorbed = false;

        }
    }

}
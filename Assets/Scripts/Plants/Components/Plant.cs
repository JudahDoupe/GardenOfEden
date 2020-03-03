using UnityEngine;

public class Plant : MonoBehaviour
{
    public int PlantId;
    public PlantDna Dna;
    public IGrowthState GrowthState;
    public Structure Trunk { get; set; }
    public Root Roots { get; set; }

    public float PlantedDate;
    public float LastUpdatedDate;
    public float AgeInDay => LastUpdatedDate - PlantedDate;

    public bool IsAlive = true;
    public bool IsGrowing = false;
    public bool IsMature => Trunk.IsMature;

    public int TotalStructures => transform.GetComponentsInChildren<Structure>()?.Length ?? 1;
    public float RootRadius => 10 * Mathf.Sqrt(TotalStructures) / Mathf.PI;
    public Volume SustainingSugar => Volume.FromCubicMeters(0.01f * TotalStructures); //TODO: store this in the structure


    public Area StoredLight; //TODO: store light in a more useful unit
    public bool HasLightBeenAbsorbed { get; set; }

    public Volume WaterCapacity => Volume.FromCubicMeters(TotalStructures / 5); //TODO: calulate this based on structures
    public Volume StoredWater;
    public bool HasWaterBeenAbsorbed { get; set; }

    public Volume StarchCapacity => Volume.FromCubicMeters(TotalStructures / 5); //TODO: calulate this based on structures
    public Volume StoredStarch;
    public bool HasStarchBeenGenerated { get; set; }


    void Start()
    {
        Trunk = Structure.Create(this, 1);
        Trunk.transform.parent = transform;
        Trunk.transform.localPosition = Vector3.zero;
        Trunk.transform.localRotation = Quaternion.identity;
        Roots = Structure.Create(this, 0) as Root;
        Roots.transform.parent = transform;
        Roots.transform.localPosition = Vector3.zero;
        Roots.transform.localRotation = Quaternion.identity;

        PlantedDate = EnvironmentApi.GetDate();
        LastUpdatedDate = PlantedDate;
        GrowthState = new PrimaryGrowthState();

        DI.LightService.AddLightAbsorber(this, 
            (absorbedLight) => {
                HasLightBeenAbsorbed = true;
                StoredLight += absorbedLight;
                TryPhotosynthesis();
            });

        PlantApi.StartPlantGrowth(this);
    }

    public void Die()
    {
        DI.GrowthService.StopPlantGrowth(this);
        IsAlive = false;
        Destroy(gameObject);
    }

    public void TryPhotosynthesis()
    {
        if (HasLightBeenAbsorbed && HasWaterBeenAbsorbed)
        {
            if (StoredStarch < StarchCapacity)
            {
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
            }

            StoredLight = Area.FromSquareMeters(0);
            HasWaterBeenAbsorbed = false;
            HasLightBeenAbsorbed = false;
            HasStarchBeenGenerated = true;
        }
    }

}
using UnityEngine;

public class Plant : MonoBehaviour
{
    public int Id;
    public PlantDNA DNA;
    public IGrowthState GrowthState;
    public UnitsOfWater StoredWater;

    public float PlantedDate;
    public float LastUpdatedDate;
    public float AgeInDay => LastUpdatedDate - PlantedDate;

    public bool IsAlive;
    public bool IsGrowing;
    public bool IsMature => Trunk.IsFullyGrown;
    public float RootRadius => Mathf.Sqrt(10 * (transform.GetComponentsInChildren<Structure>()?.Length ?? 1)) / Mathf.PI;

    public Structure Trunk;

    public PlantDNA GenerateDNA()
    {
        return new PlantDNA
        {
            Name = DNA.Name,
            Trunk = Trunk.GenerateDNA(),
            GestationPeriod = DNA.GestationPeriod,
            MaxOffspring = DNA.MaxOffspring,
            SpeciesId = DNA.SpeciesId,
            RootRadius = RootRadius
        };
    }

}
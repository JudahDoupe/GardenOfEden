using UnityEngine;

public class Plant : MonoBehaviour
{
    public int Id;
    public PlantDNA DNA;
    public IGrowthState GrowthState;
    public Volume StoredWater;
    public Volume StoredSugar;

    public float PlantedDate;
    public float LastUpdatedDate;
    public float AgeInDay => LastUpdatedDate - PlantedDate;

    public bool IsAlive;
    public bool IsGrowing;
    public bool IsMature => Trunk.IsFullyGrown;

    public int TotalStructures => transform.GetComponentsInChildren<Structure>()?.Length ?? 1;
    public float RootRadius => 10 * Mathf.Sqrt(TotalStructures) / Mathf.PI;
    public Volume SustainingSugar => Volume.FromCubicMeters(0.01f * TotalStructures); //TODO: store this in the structure

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
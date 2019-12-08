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

    public Structure Trunk;

    public void Die()
    {
        IsAlive = false;
        Destroy(gameObject);
    }

    public PlantDNA GenerateDNA()
    {
        return new PlantDNA
        {
            Name = DNA.Name,
            Trunk = Trunk.GenerateDNA(),
            GestationPeriod = DNA.GestationPeriod,
            MaxOffspring = DNA.MaxOffspring,
            SpeciesId = DNA.SpeciesId,
            RootRadius = GetRootRadius()
        };
    }

    public float GetRootRadius()
    {
        var structures = transform.GetComponentsInChildren<Structure>()?.Length ?? 1;
        return Mathf.Sqrt(10 * structures / Mathf.PI);
    }
}
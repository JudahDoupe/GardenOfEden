using UnityEngine;

public class ReproductionState : IGrowthState
{
    public void Grow(Plant plant)
    {
        var rootRadius = plant.RootRadius;

        while (plant.StoredSugar > plant.SustainingSugar + Volume.FromCubicMeters(3))
        {
            var randomLocation = Random.insideUnitSphere * rootRadius * 5;
            var worldPosition = plant.transform.position + randomLocation;

            plant.StoredSugar -= Volume.FromCubicMeters(1);
            PlantApi.DropSeed(CreateNextGeneration(plant.Dna), worldPosition);
        }

        plant.GrowthState = new SecondaryGrowthState();
    }

    private PlantDna CreateNextGeneration(PlantDna dna)
    {
        return new PlantDna
        {
            SpeciesId = dna.SpeciesId,
            Name = dna.Name,
            Generation = dna.Generation + 1,
            Resources = dna.Resources
        };
    }
}

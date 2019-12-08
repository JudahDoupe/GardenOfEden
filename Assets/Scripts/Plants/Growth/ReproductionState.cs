using UnityEngine;

public class ReproductionState : IGrowthState
{
    public void Grow(Plant plant)
    {
        var rootRadius = plant.GetRootRadius();
        for (int i = 0; i < plant.DNA.MaxOffspring; i++)
        {
            var randomLocation = Random.insideUnitSphere * rootRadius * 5;
            var worldPosition = plant.transform.position + randomLocation;

            PlantApi.TryPlantSeed(plant.DNA, worldPosition);
        }

        plant.GrowthState = new SecondaryGrowthState();
    }
}

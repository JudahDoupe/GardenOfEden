using UnityEngine;

public class ReproductionState : IGrowthState
{
    public void Grow(Plant plant)
    {
        var rootRadius = plant.RootRadius;

        while (plant.StoredSugar > plant.SustainingSugar + Volume.FromCubicMeters(1))
        {
            var randomLocation = Random.insideUnitSphere * rootRadius * 5;
            var worldPosition = plant.transform.position + randomLocation;

            plant.StoredSugar -= Volume.FromCubicMeters(1);
            PlantApi.DropSeed(plant.DNA, worldPosition);
        }

        plant.GrowthState = new SecondaryGrowthState();
    }
}

﻿using UnityEngine;

public class ReproductionState : IGrowthState
{
    public void Grow(Plant plant)
    {
        var rootsDiameter = plant.Roots.Diameter;

        while (plant.StoredStarch > plant.SustainingSugar + Volume.FromCubicMeters(3))
        {
            var randomLocation = Random.insideUnitSphere * rootsDiameter * 3;
            var worldPosition = plant.transform.position + randomLocation;

            plant.StoredStarch -= Volume.FromCubicMeters(1);
            DI.ReproductionService.DropSeed(CreateNextGeneration(plant.Dna), worldPosition);
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

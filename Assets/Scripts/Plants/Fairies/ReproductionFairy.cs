using UnityEngine;

public class ReproductionFairy : IFairy
{
    public void VisitPlant(Plant plant)
    {
        //TODO: Find all flowers for plant and try to drop seeds
    }

    public void TryDropSeeds(Plant plant, Node flower)
    {
        var plantDna = plant.Dna;
        var flowerDna = plantDna.FlowerDna;
        var daysOfSeedGrowth = flower.Age - flowerDna.DaysToMaturity - flowerDna.DaysForPolination;
        if (daysOfSeedGrowth / flowerDna.DaysToSeed >= 1)
        {
            plantDna.Generation += 1;
            var height = flower.transform.position.y - plant.transform.position.y;

            for (var i = 0; i < Random.Range(1, flowerDna.NumberOfSeeds); i++)
            {
                var randomLocation = Random.insideUnitSphere * height * 5;
                var worldPosition = flower.transform.position + randomLocation;

                DI.ReproductionService.DropSeed(plantDna, worldPosition);
            }

            flower.Kill();
        }
    }
}
using UnityEngine;

public class Flower : Node
{
    public static Flower Create(Node baseNode)
    {
        var node = Node.Create<Flower>(baseNode);

        node.Mesh = InstancedMeshRenderer.AddInstance("Flower");

        return node;
    }

    public void TryDropSeeds()  //TODO: Create a visitor for this
    {
        var plantDna = Plant.Dna;
        var flowerDna = plantDna.FlowerDna;
        var daysOfSeedGrowth = Age - flowerDna.DaysToMaturity - flowerDna.DaysForPolination;
        if (daysOfSeedGrowth / flowerDna.DaysToSeed >= 1)
        {
            plantDna.Generation += 1;
            var height = transform.position.y - Plant.transform.position.y;

            for (var i = 0; i < Random.Range(1, flowerDna.NumberOfSeeds); i++)
            {
                var randomLocation = Random.insideUnitSphere * height * 5;
                var worldPosition = transform.position + randomLocation;

                DI.ReproductionService.DropSeed(plantDna, worldPosition);
            }

            Kill();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : TimeTracker
{
    public Node Node;
    public Plant Plant;
    public FlowerDna Dna;
    public RenderingInstanceData Mesh;

    public float Size;

    public static Flower Create(Node node)
    {
        var dna = node.Plant.Dna.FlowerDna;
        var flower = new GameObject("flower").AddComponent<Flower>();

        flower.transform.parent = node.transform;
        flower.transform.localPosition = new Vector3(0, 0, 0);
        flower.transform.localRotation = Quaternion.identity;
        flower.gameObject.AddComponent<Rigidbody>().isKinematic = true;
        flower.Mesh = InstancedMeshRenderer.AddInstance("Flower");

        flower.Node = node;
        flower.Plant = node.Plant;
        flower.Dna = dna;

        flower.CreationDate = node.CreationDate;
        flower.LastUpdateDate = node.LastUpdateDate;

        return flower;
    }

    protected bool IsMature = false;
    protected bool IsPolinated = false;
    public void Grow()
    {
        LastUpdateDate = EnvironmentApi.GetDate();

        var percentGrown = Mathf.Min(Age / Dna.DaysToMaturity, 1);
        var primaryGrowth = Mathf.Pow(percentGrown, 2);
        var flowerGrowth = primaryGrowth * percentGrown;
        flowerGrowth = float.IsNaN(flowerGrowth) ? 0 : flowerGrowth;
        Size = flowerGrowth * Dna.Size;
        Mesh.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(Size, Size, Size));

        TryDropSeeds(Plant.Dna);
    }

    private void TryDropSeeds(PlantDna dna)
    {
        var daysOfSeedGrowth = Age - Dna.DaysToMaturity - Dna.DaysForPolination;
        if (daysOfSeedGrowth / Dna.DaysToSeed >= 1)
        {
            dna.Generation += 1;
            var height = transform.position.y - Plant.transform.position.y;

            for (var i = 0; i < Random.Range(1,Dna.NumberOfSeeds); i++)
            {
                var randomLocation = Random.insideUnitSphere * height * 5;
                var worldPosition = transform.position + randomLocation;

                DI.ReproductionService.DropSeed(dna, worldPosition);
            }

            Kill();
        }
    }

    public void Kill()
    {
        Node.Flower = null;
        InstancedMeshRenderer.RemoveInstance(Mesh);
        Destroy(gameObject);
    }
}

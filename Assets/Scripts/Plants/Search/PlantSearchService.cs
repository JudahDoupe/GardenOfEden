using System.Collections.Generic;
using System.Linq;
using KdTree;
using KdTree.Math;
using UnityEngine;

public class PlantSearchService : MonoBehaviour
{
    private readonly KdTree<float, Plant> _kdTree = new KdTree<float, Plant>(3, new FloatMath());
    
    public Plant GetClosestPlant(Vector3 position)
    {
        return GetClosestPlants(position, 1).FirstOrDefault();
    }
    
    public IEnumerable<Plant> GetClosestPlants(Vector3 position, int count)
    {
        return _kdTree.GetNearestNeighbours(position.ToFloatArray(), count).Select(x => x.Value);
    }

    public IEnumerable<Plant> GetAllPlants()
    {
        return _kdTree.Select(x => x.Value);
    }

    public IEnumerable<Plant> GetPlantsWithinRadius(Vector3 position, float radius, int? speciesId = null)
    {
        return _kdTree.RadialSearch(position.ToFloatArray(), radius)
            .Select(x => x.Value)
            .Where(x => x.PlantDna.SpeciesId == (speciesId ?? x.PlantDna.SpeciesId));
    }

    private void Start()
    {
        PlantMessageBus.NewPlant.Subscribe(x =>
        {
            AddToTree(x, _kdTree);
        });
        PlantMessageBus.PlantDeath.Subscribe(x =>
        {
            _kdTree.RemoveAt(_kdTree.FindValue(x));
        });
    }

    private void AddToTree(Plant plant, KdTree<float, Plant> tree)
    {
        tree.Add(plant.transform.position.ToFloatArray(), plant);
        if (tree.Count % 100 == 0)
        {
            tree.Balance();
        }
    }
}
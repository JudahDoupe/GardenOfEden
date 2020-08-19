using System.Collections.Generic;
using System.Linq;
using KdTree;
using KdTree.Math;
using UnityEngine;

public class PlantSearchService : MonoBehaviour
{
    private readonly KdTree<float, Plant> _kdTree = new KdTree<float, Plant>(3, new FloatMath());
    private readonly Dictionary<int, KdTree<float, Plant>> _speciesTrees = new Dictionary<int, KdTree<float, Plant>>();
    
    public Plant GetClosestPlant(Vector3 position, int? speciesId = null)
    {
        return GetClosestPlants(position, 1, speciesId).FirstOrDefault();
    }
    
    public IEnumerable<Plant> GetClosestPlants(Vector3 position, int count, int? speciesId = null)
    {
        var tree = speciesId.HasValue && _speciesTrees.ContainsKey(speciesId.Value) ? _speciesTrees[speciesId.Value] : _kdTree;
        return tree.GetNearestNeighbours(position.ToFloatArray(), count)
            .Select(x => x.Value)
            .Where(x => x.PlantDna.SpeciesId == (speciesId ?? x.PlantDna.SpeciesId));
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
            if (!_speciesTrees.ContainsKey(x.PlantDna.SpeciesId))
            {
                _speciesTrees[x.PlantDna.SpeciesId] = new KdTree<float, Plant>(3, new FloatMath());
            } 
            AddToTree(x,  _speciesTrees[x.PlantDna.SpeciesId]);
        });
        PlantMessageBus.PlantDeath.Subscribe(x =>
        {
            _kdTree.RemoveAt(_kdTree.FindValue(x));
            _speciesTrees[x.PlantDna.SpeciesId].RemoveAt(_speciesTrees[x.PlantDna.SpeciesId].FindValue(x));
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
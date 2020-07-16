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

    public IEnumerable<Plant> GetPlantsWithinRadius(Vector3 position, float radius)
    {
        return _kdTree.RadialSearch(position.ToFloatArray(), radius).Select(x => x.Value);
    }
    
    private void Start()
    {
        NewPlantEventBus.Subscribe(x => _kdTree.Add(x.transform.position.ToFloatArray(), x) );
        PlantDeathEventBus.Subscribe(x => _kdTree.RemoveAt(_kdTree.FindValue(x)));
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    public int NumPlants = 1000;
    public float Radius = 200;
    public Plant Plant;
    public bool SpawnPlants = false;
    private void Update()
    {
        if (SpawnPlants)
        {
            SpawnPlants = false;
            for(var i = 0; i < NumPlants; i++)
            {
                var location = transform.position + new Vector3
                {
                    x = Random.Range(-Radius, Radius),
                    y = 0,
                    z = Random.Range(-Radius, Radius)
                };
                DI.ReproductionService.DropSeed(Plant.PlantDna, location);
            }
        }
    }
}

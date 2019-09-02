using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoneButton : MonoBehaviour
{
    public List<Transform> SpawnLocations = new List<Transform>();

    public void Clicked()
    {
        var pedestal = transform.parent.GetComponent<PlantCreationPedestal>();

        foreach (var spawnLocation in SpawnLocations)
        {
            Plant.Create(pedestal.Plant.GetDNA(), spawnLocation.position);
        }

        pedestal.EndCreation();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Plant : MonoBehaviour
{
    public bool IsManipulatable;
    public float Age;
    public Structure Trunk;

    private bool _hasReproduced;
    
    public void Update()
    {
        if (!IsManipulatable)
        {
            var daysPast = Time.smoothDeltaTime / 3f;
            Age += daysPast;
            Trunk.Grow(daysPast);
            if (Age > 2 && !_hasReproduced)
            {
                Reproduce();
            }
        }
    }

    public static Plant Create(PlantDNA dna, Vector3 worldPosition)
    {
        var plantObj = new GameObject("plant");
        plantObj.transform.position = worldPosition;
        plantObj.transform.localEulerAngles = new Vector3(-90, Random.Range(0, 365), 0);
        var plant = plantObj.AddComponent<Plant>();
        plant.IsManipulatable = false;

        var trunk = Structure.Create(plant, dna.Trunk);
        trunk.transform.parent = plantObj.transform;
        trunk.transform.localPosition = Vector3.zero;
        trunk.transform.localEulerAngles = Vector3.zero;
        plant.Trunk = trunk;

        return plant;
    }

    public void Reproduce()
    {
        var randomLocation = Random.insideUnitSphere;
        randomLocation.Scale(new Vector3(5, 0, 5));
        var worldPosition = transform.position + randomLocation;

        if (Physics.OverlapSphere(worldPosition, 1).All(x => x.gameObject.GetComponent<Plant>() == null))
        {
            Create(GetDNA(), worldPosition);
            _hasReproduced = true;
        }

    }

    public PlantDNA GetDNA()
    {
        return new PlantDNA
        {
            Trunk = Trunk.GetDNA()
        };
    }
}
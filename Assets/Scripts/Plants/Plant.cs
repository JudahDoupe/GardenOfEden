using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Plant : MonoBehaviour
{
    public const float RootRadius = 2;

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

    public static Plant Create(PlantDNA dna, Vector3 worldPosition, bool isManipulatable = false)
    {
        var plantObj = new GameObject("plant");
        plantObj.transform.position = worldPosition;
        plantObj.transform.localEulerAngles = new Vector3(-90, Random.Range(0, 365), 0);
        var plant = plantObj.AddComponent<Plant>();
        plant.IsManipulatable = isManipulatable;

        var trunk = Structure.Create(plant, dna.Trunk);
        trunk.transform.parent = plantObj.transform;
        trunk.transform.localPosition = Vector3.zero;
        trunk.transform.localEulerAngles = Vector3.zero;
        plant.Trunk = trunk;

        return plant;
    }

    public void Reproduce()
    {
        for (int i = 0; i < 4; i++)
        {
            var randomLocation = Random.insideUnitSphere * 10;
            var worldPosition = transform.position + randomLocation;
            var ray = new Ray(worldPosition, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit) &&
                hit.transform.gameObject.layer == 8 && 
                Physics.OverlapSphere(hit.point, RootRadius).All(x => x.gameObject.GetComponent<Plant>() == null))
            {
                _hasReproduced = true;
                Create(GetDNA(), hit.point);
            }
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public bool IsManipulatable;
    public float Age;
    public Structure Trunk;

	public void Start()
	{
	    Age = IsManipulatable ? 1 : 0;
        transform.localEulerAngles = new Vector3(-90,0,0);
	}
    public void Update()
    {
        if (!IsManipulatable)
        {
            var daysPast = Time.smoothDeltaTime / 3f;
            Age += daysPast;
            Trunk.Grow(daysPast);
        }
    }

    public static Plant Create(PlantDNA dna, Vector3 worldPosition)
    {
        var plantObj = new GameObject("plant");
        plantObj.transform.position = worldPosition;
        var plant = plantObj.AddComponent<Plant>();
        plant.IsManipulatable = false;

        var trunk = Structure.Create(plant, dna.Trunk);
        trunk.transform.parent = plantObj.transform;
        trunk.transform.localPosition = Vector3.zero;
        trunk.transform.localEulerAngles = Vector3.zero;
        plant.Trunk = trunk;

        return plant;
    }

    public PlantDNA GetDNA()
    {
        return new PlantDNA
        {
            Trunk = Trunk.GetDNA()
        };
    }
}
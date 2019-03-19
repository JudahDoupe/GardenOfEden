using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public bool IsManipulatable;
    public float Age;
    public List<GameObject> Structures;

    public Joint Root { get; set; }

	public void Start()
	{
	    Age = IsManipulatable ? 1 : 0;
        transform.localEulerAngles = new Vector3(-90,0,0);
        if(Root == null)Root = Joint.Build(this, null);
	}
    public void Update()
    {
        if (!IsManipulatable) Age += Time.smoothDeltaTime / 3f;
    }
    public static Plant Build(Vector3 worldPosition, PlantDTO dto)
    {
        var plantObj = new GameObject("plant");
        plantObj.transform.position = worldPosition;
        var plant = plantObj.AddComponent<Plant>();
        Destroy(plant.Root);
        plant.Root = Joint.Build(plant,null,dto.RootJoint);
        return plant;
    }
}

[Serializable]
public class PlantDTO
{
    public JointDTO RootJoint;

    public PlantDTO(Plant plant)
    {
        RootJoint = new JointDTO(null, plant.Root);
    }
}
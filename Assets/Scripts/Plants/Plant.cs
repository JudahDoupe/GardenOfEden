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
    public int StructureIndex { get; set; }

	public void Start()
	{
	    Age = IsManipulatable ? 1 : 0;
        transform.localEulerAngles = new Vector3(-90,0,0);
        if(Root == null)Root = Joint.Build(this, null);
	    ClearStructure();
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

    public void Reproduce()
    {
        var pos = transform.position + new Vector3(5, 0, 5);
        Build(pos, new PlantDTO(this));
    }

    public GameObject GetStructure()
    {
        var structure = StructureIndex < 0 ? null : Structures[StructureIndex];
        return structure;
    }
    public void ClearStructure()
    {
        StructureIndex = -1;
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
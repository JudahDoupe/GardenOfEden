using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plant : MonoBehaviour
{
    [HideInInspector]
    public Joint Root; 
    public bool IsManipulatable;

	public void Start()
	{
	    Root = Joint.Build(null, this);
	}

    public List<GameObject> Structures;
    public int StructureIndex = -1;

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

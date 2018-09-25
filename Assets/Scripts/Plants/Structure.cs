using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public float Age;
    public float Length = 1;
    public float Girth = 1;
    public PlantStructureType Type;

    public Joint Root { get; set; }
    public Joint Head { get; set; }
    public GameObject Model { get; set; }
    public Plant Plant { get; set; }

    public void Start()
    {
        Model = gameObject.transform.Find("Model").gameObject;
        Model.transform.localScale = new Vector3(Girth,Girth,Length);
        transform.localPosition = Vector3.zero;
    }

    public static Structure Build(Joint root, GameObject prefab, Plant plant)
    {
        var obj = Instantiate(prefab, root.transform);
        var structure = obj.GetComponent<Structure>();

        if (structure == null)
            Debug.Log("You forgot to add a Structure component to your prefab DUMBASS!!!");

        structure.Head = Joint.Build(structure, plant);
        structure.Root = root;
        return structure;
    }

    public void Grow(float time)
    {
        Age += time;

        //primary growth
        var size = 1 / (1 + Mathf.Exp(5 - 10 * Age));
        transform.localScale = new Vector3(size, size, size);

        //secondary growth
        if (!(size > 1)) return;
        var s = Model.transform.localScale;
        Model.transform.localScale = s + new Vector3(1 / s.x, 1 / s.y, 1 / s.z);

        Head.ResetPosition();
    }
}

public enum PlantStructureType
{
    Stem,
    Leaf,
    Root
}
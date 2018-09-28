using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
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
    public GameObject Prefab { get; set; }
    public Plant Plant { get; set; }

    public void Start()
    {
        Model = gameObject.transform.Find("Model").gameObject;
        Model.transform.localScale = new Vector3(Girth, Girth, Length);
        transform.localPosition = Vector3.zero;
    }
    public void Update()
    {
        Length = Vector3.Distance(transform.position, Head.transform.position);
        Model.transform.localScale = new Vector3(Girth, Girth, Length);
    }

    public static Structure Build(Plant plant, Joint root, GameObject prefab)
    {
        var obj = Instantiate(prefab, root.transform);
        var structure = obj.GetComponent<Structure>();

        if (structure == null)
            Debug.Log("You forgot to add a Structure component to your prefab DUMBASS!!!");

        structure.Prefab = prefab;
        structure.Head = Joint.Build(plant, structure);
        structure.Root = root;
        return structure;
    }
    public static Structure Build(Plant plant, Joint root, StructureDTO dto)
    {
        var structure = Build(plant, root, dto.Prefab);
        structure.transform.localRotation = dto.LocalRotation;
        structure.Length = dto.Length;
        structure.Girth = dto.Girth;
        Destroy(structure.Head.gameObject);
        structure.Head = Joint.Build(plant, structure, dto.ToJoint);
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

[Serializable]
public class StructureDTO
{
    public JointDTO ToJoint;
    public GameObject Prefab;
    public Quaternion LocalRotation;
    public float Length;
    public float Girth;

    public StructureDTO(Structure structure)
    {
        ToJoint = new JointDTO(this, structure.Head);
        Prefab = structure.Prefab;
        LocalRotation = structure.transform.localRotation;
        Length = structure.Length;
        Girth = structure.Girth;
    }

}

public enum PlantStructureType
{
    Stem,
    Leaf,
    Root
}
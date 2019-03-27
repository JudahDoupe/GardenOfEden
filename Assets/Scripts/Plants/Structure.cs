using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEditor;
using UnityEngine;

public class Structure : BuildingMaterial
{
    public float Age = 0;
    public float Length = 1;
    public float Girth = 1;
    public GameObject Prefab;
    public PlantStructureType Type;

    public float SproutingAge { get; set; }
    public Joint Root { get; set; }
    public Joint Head { get; set; }
    public GameObject Model { get; set; }
    public Plant Plant { get; set; }

    public void Start()
    {
        Model = gameObject.transform.Find("Model").gameObject;
        Model.transform.localScale = new Vector3(Girth, Girth, Length);
    }
    public void Update()
    {
        if (Plant != null)
        {
            Age = Mathf.Max(Plant.Age - SproutingAge,0);
            var growth = 1 / (1 + Mathf.Exp(5 - 10 * Age));

            transform.localScale = new Vector3(growth, growth, growth);
            Model.transform.localScale = new Vector3(Girth, Girth, Length);
            if (growth > 1) Model.transform.localScale += new Vector3(1 / Girth, 1 / Girth, 1 / Length);
        }
    }

    public static Structure BuildPrefab(Plant plant, Joint root, GameObject prefab)
    {
        var obj = Instantiate(prefab, root.transform);
        obj.transform.localPosition = Vector3.zero;
        var structure = obj.GetComponent<Structure>();

        if (structure == null)
            Debug.Log("You forgot to add a Structure component to your prefab DUMBASS!!!");

        structure.Plant = plant;
        structure.SproutingAge = plant.Age - 1;
        structure.Prefab = prefab;
        structure.Head = Joint.BuildNew(plant, structure);
        structure.Root = root;
        return structure;
    }
    public static Structure BuildDto(Plant plant, Joint root, StructureDTO dto)
    {
        var structure = BuildPrefab(plant, root, dto.Prefab);
        structure.transform.localRotation = dto.LocalRotation;
        structure.Length = dto.Length;
        structure.Girth = dto.Girth;
        structure.SproutingAge = dto.SproutingAge;
        Destroy(structure.Head.gameObject);
        structure.Head = Joint.BuildDto(plant, structure, dto.ToJoint);
        return structure;
    }
    public Structure Disconnect()
    {
        Root?.Disconnect(this);
        Head?.Disconnect(this);
        Root = null;
        Head = null;
        Plant = null;
        transform.parent = null;
        return this;
    }

    public override bool IsInteractable(FirstPersonController player)
    {
        return (player.Tool == null && Plant == null) ||
               player.Tool is Axe ||
               player.Tool is BranchBeefer;
    }
    public override void Interact(FirstPersonController player)
    {
        if (Type == PlantStructureType.Stem)
        {
            if (player.Material == null && Plant == null)
            {
                player.GrabMaterial(Disconnect());
            }
        }
        else
        {
            if (player.Material == null)
            {
                player.GrabMaterial(Disconnect());
            }
        }
    }
    public override Vector3 InteractionPosition()
    {
        return transform.Find("Model")?.GetChild(0)?.transform.position ?? transform.position;
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
    public float SproutingAge;

    public StructureDTO(Structure structure)
    {
        ToJoint = new JointDTO(this, structure.Head);
        Prefab = structure.Prefab;
        LocalRotation = structure.transform.localRotation;
        Length = structure.Length;
        Girth = structure.Girth;
        SproutingAge = structure.SproutingAge;
    }

}

public enum PlantStructureType
{
    Stem,
    Leaf,
    Root
}
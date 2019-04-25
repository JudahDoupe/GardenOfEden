using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEditor;
using UnityEngine;

public class Structure : Item
{
    public float Age = 0;
    public float Length = 1;
    public float Girth = 1;
    public GameObject Prefab;
    public PlantStructureType Type;

    public float SproutingAge { get; set; }
    public List<Connection> Connections { get; set; }
    public Connection BaseConnection { get; set; }
    public GameObject Model { get; set; }
    public Plant Plant { get; set; }

    public void Start()
    {
        Model = gameObject.transform.Find("Model").gameObject;
        Model.transform.localScale = new Vector3(Girth, Girth, Length);
        Connections = new List<Connection>();
        Age = Length / 2 + Girth;

        var t = transform;
        while (t != null && (t.GetComponent<Plant>() == null))
        {
            t = t.parent;
        }
        Plant = t?.GetComponent<Plant>();
    }
    public void Update()
    {
        if (Plant != null && !Plant.IsManipulatable)
        {
            Age = Mathf.Max(Plant.Age - SproutingAge,0);
        }

        var growth = 1 / (1 + Mathf.Exp(5 - 10 * Age));
        transform.localScale = new Vector3(growth, growth, growth);
        Model.transform.localScale = new Vector3(Girth, Girth, Length);
        if (growth > 1) Model.transform.localScale += new Vector3(1 / Girth, 1 / Girth, 1 / Length);
    }

    public static Structure Create(Plant plant, GameObject prefab)
    {
        var obj = Instantiate(prefab);
        obj.transform.localPosition = Vector3.zero;
        var structure = obj.GetComponent<Structure>();

        if (structure == null)
            Debug.Log("You forgot to add a Structure component to your prefab DUMBASS!!!");

        structure.Plant = plant;
        structure.SproutingAge = plant.Age - 1;
        structure.Prefab = prefab;
        structure.Connections = new List<Connection>();
        return structure;
    }
    public static Structure Create(Plant plant, PlantDNA.Structure dna)
    {
        var structure = Create(plant, dna.Prefab);
        structure.Girth = dna.Girth;
        structure.Length = dna.Length;

        foreach (var dnaConnection in dna.Connections)
        {
            Connection.Create(structure, dnaConnection);
        }

        return structure;
    }

    public Connection Connect(Structure structure, Vector3 localPosition)
    {
        var rotation = Quaternion.FromToRotation(Vector3.up, InteractionPosition() - localPosition);
        var connection = Connection.Create(this, structure, localPosition, rotation);
        Connections.Add(connection);
        Destroy(structure.GetComponent<Rigidbody>());
        return connection;
    }

    public PlantDNA.Structure GetDNA()
    {
        return new PlantDNA.Structure
        {
            Prefab = Prefab,
            Length = Length,
            Girth = Girth,
            Connections = Connections.Select(c => c.GetDNA()).ToList()
        };
    }

    public override bool IsUsable(FirstPersonController player, Interactable interactable)
    {
        return interactable is Structure && (interactable as Structure).Plant != null;
    }
    public override void Use(FirstPersonController player, Interactable interactable)
    {
        if (interactable is Structure structure)
        {
            player.DropItem(this);
            var localPos = structure.transform.worldToLocalMatrix * player.Focus.transform.position;
            structure.Connect(this, localPos);
        }
    }
    public override bool IsInteractable(FirstPersonController player)
    {
        return BaseConnection == null && transform.parent?.GetComponent<Plant>() == null;
    }
    public override Vector3 InteractionPosition()
    {
        return transform.Find("Model")?.GetChild(0)?.transform.position ?? transform.position;
    }
}

public enum PlantStructureType
{
    Stem,
    Leaf,
    Root
}
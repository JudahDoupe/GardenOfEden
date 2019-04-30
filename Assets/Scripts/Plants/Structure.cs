﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEditor;
using UnityEngine;

public class Structure : Item
{
    private const float SecondaryGrowthSpeed = 20;
    private const float DaysToMaturity = 1;

    public float DaysOld = 0;
    public float Length = 1;
    public float Girth = 1;
    public GameObject Prefab;
    public PlantStructureType Type;

    public Plant Plant;
    public GameObject Model;
    public List<Connection> Connections { get; set; } = new List<Connection>();
    public Connection BaseConnection { get; set; }
    public PlantDNA.Structure DNA { get; set; }

    private bool _hasSprouted = false;

    public static Structure Create(Plant plant, GameObject prefab)
    {
        var obj = Instantiate(prefab);
        obj.transform.localPosition = Vector3.zero;
        var structure = obj.GetComponent<Structure>();

        if (structure == null)
            Debug.Log("You forgot to add a Structure component to your prefab DUMBASS!!!");

        structure.Plant = plant;
        structure.Prefab = prefab;
        structure.Model = structure.transform.Find("Model").gameObject;
        return structure;
    }
    public static Structure Create(Plant plant, PlantDNA.Structure dna)
    {
        var structure = Create(plant, dna.Prefab);
        structure.Girth = dna.Girth;
        structure.Length = dna.Length;
        structure.DNA = dna;

        return structure;
    }

    public void Grow(float days)
    {
        DaysOld += days;

        UpdateModel();

        if (!_hasSprouted && DaysOld > DaysToMaturity)
        {
            foreach (var connection in DNA.Connections)
            {
                Connection.Create(this, connection);
            }

            _hasSprouted = true;
        }

        Connections.ForEach(c => c.To.Grow(days));
    }

    public void UpdateModel()
    {
        var primaryGrowth = 1 / (1 + Mathf.Exp(5 - 10 / DaysToMaturity * DaysOld));
        var secondaryGrowth = 1 + DaysOld / SecondaryGrowthSpeed;

        transform.localScale = new Vector3(primaryGrowth, primaryGrowth, primaryGrowth);
        Model.transform.localScale = new Vector3(Girth * secondaryGrowth, Girth * secondaryGrowth, Length);
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
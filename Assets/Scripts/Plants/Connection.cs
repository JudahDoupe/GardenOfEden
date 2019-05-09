using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Connection : Interactable
{
    public Structure From;
    public Structure To;

    public static Connection Create(Structure from, Structure to, Vector3 localPosition, Quaternion localRotation)
    {
        var connection = new GameObject("Connection", typeof(Connection)).GetComponent<Connection>();
        connection.transform.parent = from.transform;
        connection.transform.localScale = Vector3.one;
        connection.transform.localPosition = localPosition;
        connection.transform.localRotation = localRotation;

        to.Plant = from.Plant;
        to.BaseConnection = connection;
        to.transform.parent = connection.transform;
        to.transform.localPosition = Vector3.zero;
        to.transform.localRotation = Quaternion.identity;

        connection.To = to;
        connection.From = from;

        from.Connections.Add(connection);

        return connection;
    }
    public static Connection Create(Structure from, PlantDNA.Connection dna)
    {
        return Create(from, Structure.Create(from.Plant, dna.Structure), dna.Position, dna.Rotation);
    }

    public void Break()
    {
        To.BaseConnection = null;
        To.transform.parent = null;
        To.Plant = null;

        From.Connections.Remove(this);
        Destroy(gameObject);
    }

    public PlantDNA.Connection GetDNA()
    {
        return new PlantDNA.Connection
        {
            Position = transform.localPosition,
            Rotation = transform.localRotation,
            Structure = To.GetDNA()
        };
    }

}
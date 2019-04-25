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

    public void Update()
    {
        if (From != null)
        {
            transform.localPosition = new Vector3(0, 0, From.Length);
        }
    }

    public static Connection Create(Structure from, Structure to, Vector3 localPosition, Quaternion localRotation)
    {
        var model = new GameObject("Connection");
        model.AddComponent<SphereCollider>();
        var connection = model.AddComponent<Connection>();
        connection.GetComponent<SphereCollider>().isTrigger = true;
        Destroy(connection.GetComponent<Renderer>());

        connection.From = from;
        connection.transform.parent = from.transform;
        connection.transform.localScale = connection.From.Plant.IsManipulatable ? Vector3.one : Vector3.one * from.Girth;
        connection.transform.localPosition = localPosition;
        connection.transform.localRotation = localRotation;
        connection.To = to;
        connection.To.Plant = connection.From.Plant;
        connection.To.BaseConnection = connection;
        connection.To.transform.parent = connection.transform;
        connection.To.transform.localPosition = Vector3.zero;
        connection.To.transform.localRotation = Quaternion.identity;

        return connection;
    }
    public static Connection Create(Structure from, PlantDNA.Connection dna)
    {

        var connection = Create(from, Structure.Create(from.Plant, dna.Structure), dna.Position, dna.Rotation);

        return connection;
    }
    public void Break()
    {
        To.BaseConnection = null;
        To.transform.parent = null;
        To.Plant = null;
        To.Fall();

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

    public void SetPosition(Vector3 position)
    {
        transform.position = position;

        From.Length = Vector3.Distance(transform.position, From.transform.position);
        From.transform.LookAt(transform);
        transform.rotation = From.transform.rotation;
    }
    public override bool IsInteractable(FirstPersonController player)
    {
        return false;
    }

    private IEnumerator Fall()
    {
        var rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.angularDrag *= 10;
        rigidbody.drag *= 5;
        yield return new WaitForSeconds(1);
        while (rigidbody.velocity.magnitude > 0.0001f)
        {
            yield return new WaitForEndOfFrame();
        }
        Destroy(rigidbody);
    }

}
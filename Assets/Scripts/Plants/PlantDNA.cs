using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlantDNA : MonoBehaviour
{
    public Structure Trunk;
    public string Name;

    [Serializable]
    public class Structure
    {
        public PlantStructureType Type;
        public GameObject Prefab;
        public float Length;
        public float Diameter;
        public List<Connection> Connections;
    }

    [Serializable]
    public class Connection
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Structure Structure;
    }
}

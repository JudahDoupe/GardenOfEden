using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantDNA : MonoBehaviour
{
    public Structure Trunk;

    public class Structure
    {
        public GameObject Prefab;
        public float Length;
        public float Girth;
        public List<Connection> Connections;
    }

    public class Connection
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Structure Structure;
    }
}

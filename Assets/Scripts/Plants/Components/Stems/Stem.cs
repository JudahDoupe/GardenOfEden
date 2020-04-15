using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stem : TimeTracker
{
    public Node Node;
    public Plant Plant;
    public StemDna Dna;
    public MeshData Mesh;

    public float Length;
    public float Radius;

    public static Stem Create(Node node)
    {
        var dna = node.Plant.Dna.StemDna;
        var stem = new GameObject(dna.Type.ToString()).AddComponent<Stem>();

        stem.transform.parent = node.transform;
        stem.transform.localPosition = new Vector3(0, 0, 0);
        stem.transform.localRotation = Quaternion.identity;
        stem.gameObject.AddComponent<Rigidbody>().isKinematic = true;
        stem.gameObject.AddComponent<MeshRenderer>().material = dna.Material;
        stem.Mesh = new MeshData(stem.gameObject.AddComponent<MeshFilter>().mesh, 5);

        stem.Node = node;
        stem.Plant = node.Plant;
        stem.Dna = dna;

        stem.CreationDate = node.CreationDate;
        stem.LastUpdateDate = node.LastUpdateDate;

        return stem;
    }

    void OnTriggerEnter(Collider collider)
    {
        Plant plant = collider.transform.GetComponentInParent<Plant>();

        if (plant != Node.Plant)
        {
            Node.Kill();
        }
    }

    public Volume Grow(Volume availableSugar) //TODO: use sugar
    {
        LastUpdateDate = EnvironmentApi.GetDate();

        var percentGrown = Age / Dna.DaysToMaturity;
        var primaryGrowth = Mathf.Pow(percentGrown, 2);
        var secondaryGrowth = Mathf.Pow(percentGrown, 1.2f) / percentGrown;
        var growth = Mathf.Lerp(primaryGrowth, secondaryGrowth, percentGrown);
        growth = float.IsNaN(growth) ? 0 : growth;

        Length = Dna.PrimaryLength * growth;
        Radius = Dna.PrimaryRadius * growth;

        UpdateMesh();

        return availableSugar;
    }

    public void UpdateMesh()
    {
        var baseStem = Node.BaseNode == null ? null : Node.BaseNode.Stem.Mesh;

        Mesh.Center.Bottom = new Vector3(0, 0, -Length);
        for (int i = 0; i < Mesh.Sides.Length; i++)
        {
            Mesh.Sides[i].Top = Mesh.Sides[i].Direction * Radius;
            Mesh.Sides[i].Bottom = (baseStem ?? Mesh).Sides[i].Top + Mesh.Center.Bottom;
        }

        Mesh.UpdateMesh();
        Node.transform.localPosition = Node.transform.localRotation * Vector3.forward * Length;
    }


    public class MeshData
    {
        public Mesh Mesh;
        public Side Center;
        public Side[] Sides;
        public struct Side
        {
            public Vector3 Top;
            public Vector3 Bottom;
            public Vector3 Direction;
        }

        public MeshData(Mesh mesh, int numSides)
        {
            Mesh = mesh;

            Center = new Side
            {
                Top = new Vector3(0, 0, 0),
                Bottom = new Vector3(0, 0, -0.1f),
                Direction = new Vector3(0, 0, -1),
            };
            Sides = new Side[numSides];
            for (var i = 0; i < numSides; i++)
            {
                var a = ((2 * Mathf.PI) / numSides) * i;
                var x = Mathf.Cos(a) * 0.1f;
                var y = Mathf.Sin(a) * 0.1f;
                Sides[i] = new Side
                {
                    Top = new Vector3(x, y, -0.01f),
                    Bottom = new Vector3(x, y, -0.09f),
                    Direction = new Vector3(x, y, 0).normalized
                };
            }

            UpdateMesh();
        }

        public void UpdateMesh()
        {
            Mesh.Clear();
            Mesh.vertices = getVertexArray();
            Mesh.triangles = getTriangleArray();
            Mesh.uv = getUvArray();
            Mesh.normals = getNormalArray();
            Mesh.RecalculateBounds();
        }
        private Vector3[] getVertexArray()
        {
            var verticies = new List<Vector3>
            {
                Center.Top,
                Center.Bottom
            };
            foreach (var side in Sides)
            {
                verticies.Add(side.Top);
                verticies.Add(side.Bottom);
            }
            return verticies.ToArray();
        }
        private Vector3[] getNormalArray()
        {
            var normals = new List<Vector3>
            {
                new Vector3(0,0,1),
                new Vector3(0,0,-1)
            };
            foreach (var side in Sides)
            {
                normals.Add(side.Direction);
                normals.Add(side.Direction);
            }
            return normals.ToArray();
        }
        private Vector2[] getUvArray()
        {
            var uvs = new List<Vector2>
            {
                new Vector2(0.5f,1),
                new Vector2(0.5f,0)
            };
            for (float i = 0; i < Sides.Length; i++)
            {
                var x = Mathf.Abs(((1f / Sides.Length) * i) - 0.5f);
                uvs.Add(new Vector2(x, 1));
                uvs.Add(new Vector2(x, 0));
            }
            return uvs.ToArray();
        }
        private int[] getTriangleArray()
        {
            var triangles = new List<int>();
            var topVertex = 0;
            var bottomVertex = 1;
            for (var i = 1; i < Sides.Length + 1; i++)
            {
                var topSideVertex = (2 * i);
                var bottomSideVertex = topSideVertex + 1;
                var nextTopSideVertex = (2 * ((i % Sides.Length) + 1));
                var nextBottomSideVertex = nextTopSideVertex + 1;

                triangles.AddRange(new[] { topVertex, topSideVertex, nextTopSideVertex });
                triangles.AddRange(new[] { topSideVertex, bottomSideVertex, nextBottomSideVertex });
                triangles.AddRange(new[] { nextBottomSideVertex, nextTopSideVertex, topSideVertex });
                triangles.AddRange(new[] { bottomSideVertex, bottomVertex, nextBottomSideVertex });
            }
            return triangles.ToArray();
        }
    }
}
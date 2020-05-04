using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlowerMesh : MonoBehaviour
{
    public Mesh Mesh { get; set; }
    public Edge[] Edges { get; set; }

    public class Edge
    {
        public Vector3 Top;
        public Vector3 Bottom;
        public Vector3 Vector;
        public float Theta;
    }

    public FlowerMesh(Mesh mesh, int numSides, Length L, Width W, Height H)
    {
        Mesh = mesh;
        Edges = new Edge[numSides];
        for (var i = 0; i < numSides; i++)
        {
            var theta = 2 * i * Mathf.PI / numSides;
            var height = H(theta);
            var width = W(theta);
            var length = L(theta);
            Edges[i] = new Edge
            {
                Theta = theta,
                Top = new Vector3(height, width, length),
                Bottom = new Vector3(height, width, length),
                Vector = new Vector3(height, width, length)
            };
        }

        HardUpdateMesh();
    }
    public delegate float Height(float theta);
    public delegate float Width(float theta);
    public delegate float Length(float theta);

    public void QuickUpdateMesh()
    {
        var triangles = Mesh.triangles;
        var uv = Mesh.uv;
        Mesh.Clear();
        Mesh.vertices = getVertexArray();
        Mesh.normals = getNormalArray();
        Mesh.triangles = triangles;
        Mesh.uv = uv;
        Mesh.RecalculateBounds();
    }
    public void HardUpdateMesh()
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
        return Edges.SelectMany(x => new[] { x.Top, x.Bottom }).ToArray();
    }
    private Vector3[] getNormalArray()
    {
        return Edges.SelectMany(x => new[] { new Vector3(-1, x.Vector.y, 0).normalized, new Vector3(1, x.Vector.y, 0).normalized }).ToArray();
    }
    private Vector2[] getUvArray()
    {
        return Edges.SelectMany(x => new[] { new Vector2(Mathf.Sin(x.Theta), Mathf.Cos(x.Theta)), new Vector2(Mathf.Sin(x.Theta), Mathf.Cos(x.Theta)) }).ToArray();
    }
    private int[] getTriangleArray()
    {
        var triangles = new List<int>();
        for (var i = 1; i < Edges.Length - 1; i++)
        {
            var idx = i * 2;
            triangles.AddRange(new[] { 0, idx + 2, idx });
            triangles.AddRange(new[] { 1, idx + 1, idx + 3 });
        }
        return triangles.ToArray();
    }
}

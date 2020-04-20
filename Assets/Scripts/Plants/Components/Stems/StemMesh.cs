using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StemMesh
{
    public Mesh Mesh { get; set; }
    public Side Center { get; set; }
    public Side[] Sides { get; set; }

    public class Side
    {
        public Vector3 Top;
        public Vector3 Bottom;
        public Vector3 Direction;
    }

    public StemMesh(Mesh mesh, int numSides)
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

        HardUpdateMesh();
    }

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

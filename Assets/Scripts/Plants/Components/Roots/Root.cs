using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Root : Structure
{
    [HideInInspector]
    public RootMeshData MeshData { get; private set; }

    public void Start()
    {
        MeshData = new RootMeshData(_model.GetComponentInChildren<MeshFilter>().mesh, 6);
        DI.RootService.AddRoots(this, AbsorbWater);
    }
    public void AbsorbWater(Volume water)
    {
        Plant.HasWaterBeenAbsorbed = true;
        Plant.StoredWater += water;
        Plant.TryPhotosynthesis();
    }
    public void GrowOutward(Volume volume)
    {
        var circumfrance = 0f;
        var sumDepth = 0f;

        for(var i = 0; i < MeshData.NumSides; i++)
        {
            var side = MeshData.Sides[i];
            var nextSide = MeshData.Sides[(i + 1) % MeshData.NumSides];

            var topDistance = Vector3.Distance(side.Top, nextSide.Top);
            var bottomDistance = Vector3.Distance(side.Bottom, nextSide.Bottom);

            circumfrance += (topDistance + bottomDistance) / 2f;
            sumDepth += Vector3.Distance(side.Top, side.Bottom);
        }

        var area = circumfrance * (sumDepth / MeshData.NumSides);
        var distance = volume._cubicMeters / area;

        GrowOutward(distance);
    }
    public void GrowOutward(float distance)
    {
        for (int i = 0; i < MeshData.NumSides; i++)
        {
            var direction = MeshData.Sides[i].Direction;
            MeshData.Sides[i].Top += direction * distance * 1.2f;
            MeshData.Sides[i].Bottom += direction * distance * 0.8f;
        }

        MeshData.UpdateMesh();
    }

    public void GrowDownward(float distance)
    {
        var direction = MeshData.Center.Direction;
        MeshData.Center.Bottom += direction * distance * 1.2f;
        for (int i = 0; i < MeshData.NumSides; i++)
        {
            MeshData.Sides[i].Bottom += direction * distance * 0.8f;
        }

        MeshData.UpdateMesh();
    }
}
public class RootMeshData
{
    public readonly Mesh Mesh;
    public readonly int NumSides;
    public Side Center;
    public Side[] Sides;
    public struct Side
    {
        public Vector3 Top;
        public Vector3 Bottom;
        public Vector3 Direction;
    }

    public RootMeshData(Mesh mesh, int numSides)
    {
        NumSides = numSides;
        Mesh = mesh;

        Center = new Side
        {
            Top = new Vector3(0, 0, 0),
            Bottom = new Vector3(0, 0, -0.1f),
            Direction = new Vector3(0, 0, -1),
        };
        Sides = new Side[NumSides];
        for (var i = 0; i < NumSides; i++)
        {
            var a = ((2 * Mathf.PI) / NumSides) * i;
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
        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();
    }
    private Vector3[] getVertexArray()
    {
        var verticies = new List<Vector3>();
        verticies.Add(Center.Top);
        verticies.Add(Center.Bottom);
        foreach(var side in Sides)
        {
            verticies.Add(side.Top);
            verticies.Add(side.Bottom);
        }
        return verticies.ToArray();
    }
    private int[] getTriangleArray()
    {
        var triangles = new List<int>();
        var topVertex = 0;
        var bottomVertex = 1;
        for (var i = 1; i < NumSides + 1; i++)
        {
            var topSideVertex = (2 * i);
            var bottomSideVertex = topSideVertex + 1;
            var nextTopSideVertex = (2 * ((i % NumSides) + 1));
            var nextBottomSideVertex = nextTopSideVertex + 1;

            triangles.AddRange(new[] { topVertex, topSideVertex, nextTopSideVertex });
            triangles.AddRange(new[] { topSideVertex, bottomSideVertex, nextBottomSideVertex });
            triangles.AddRange(new[] { nextBottomSideVertex, nextTopSideVertex, topSideVertex });
            triangles.AddRange(new[] { bottomSideVertex, bottomVertex, nextBottomSideVertex });
        }
        return triangles.ToArray();
    }
}

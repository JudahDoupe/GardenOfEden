using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : Structure
{
    [HideInInspector]
    public Mesh Mesh;
    public int NumPolygonSides = 6;

    public void Start()
    {
        Mesh = _model.GetComponentInChildren<MeshFilter>().mesh;
        CreateMesh();
        DI.RootService.AddRoots(this, AbsorbWater);
    }
    public void AbsorbWater(Volume water)
    {
        Plant.HasWaterBeenAbsorbed = true;
        Plant.StoredWater += water;
        Plant.TryPhotosynthesis();
    }
    public void GrowInDirection(Volume volume, Vector3 direction)
    {
        direction = direction.normalized;

        var distance = volume._cubicMeters / CalculateFacingArea(direction)._squareMeters;
        var center = Mesh.bounds.center;
        var verticies = Mesh.vertices;
        var triangles = Mesh.triangles;

        for (int i = 1; i < verticies.Length - 2; i++)
        {
            var vertex = verticies[i];
            var myDirection = (vertex - center).normalized;

            verticies[i] = vertex + direction * (distance * Mathf.Max(0, Vector3.Dot(myDirection, direction)));
        }

        UpdateMesh(verticies, triangles);
    }


    private void CreateMesh()
    {
        var verticies = new List<Vector3>();
        var triangles = new List<int>();

        verticies.Add(new Vector3(0, 0, 0)); //center top
        for(var i = 0; i < NumPolygonSides; i++)
        {
            var a = ((2 * Mathf.PI) / NumPolygonSides) * i;
            var x = Mathf.Cos(a);
            var y = Mathf.Sin(a);
            verticies.Add(new Vector3(x, y, -0.01f)); //top ring
            verticies.Add(new Vector3(x, y, -0.09f)); //bottom ring
        }
        verticies.Add(new Vector3(0, 0, -0.1f)); //center bottom

        var topVertex = 0;
        var bottomVertex = (2 * NumPolygonSides) + 1;
        for (var i = 0; i < NumPolygonSides; i++)
        {
            var topSideVertex = (2 * i) + 1;
            var bottomSideVertex = topSideVertex + 1;
            var nextTopSideVertex = (2 * ((i + 1) % NumPolygonSides)) + 1;
            var nextBottomSideVertex = nextTopSideVertex + 1;

            triangles.AddRange(new []{ topVertex, topSideVertex, nextTopSideVertex });
            triangles.AddRange(new []{ topSideVertex, bottomSideVertex, nextBottomSideVertex });
            triangles.AddRange(new []{ nextBottomSideVertex, nextTopSideVertex, topSideVertex });
            triangles.AddRange(new []{ bottomSideVertex, bottomVertex, nextBottomSideVertex });
        }

        UpdateMesh(verticies.ToArray(), triangles.ToArray());
    }
    private void UpdateMesh(Vector3[] verticies, int[] triangles)
    {
        Mesh.Clear();
        Mesh.vertices = verticies;
        Mesh.triangles = triangles;
        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();
    }
    Area CalculateFacingArea(Vector3 direction)
    {
        direction = direction.normalized;
        var triangles = Mesh.triangles;
        var vertices = Mesh.vertices;

        var sum = 0.0f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 corner = vertices[triangles[i]];
            Vector3 a = vertices[triangles[i + 1]] - corner;
            Vector3 b = vertices[triangles[i + 2]] - corner;

            float projection = Vector3.Dot(Vector3.Cross(b, a), direction);
            if (projection > 0f)
                sum += projection;
        }

        return Area.FromSquareMeters(sum / 2.0f);
    }
}

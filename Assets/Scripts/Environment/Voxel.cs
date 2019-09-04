using System;
using UnityEngine;

public class Voxel
{
    public VoxelCoord Coord { get; }
    public VoxelType Type { get; private set; }
    public float Light { get; set; }
    public float Water { get; set; }
    public float Soil { get; set; }

    public Voxel(VoxelCoord coord)
    {
        Coord = coord;
        Light = Mathf.Pow(VoxelCoord.Size, 3);
        var debugColor = Color.magenta;

        UpdateVoxelType();

        switch (Type)
        {
            case VoxelType.Land:
                Water = Mathf.Pow(VoxelCoord.Size / 4, 3);
                Soil = Mathf.Pow(VoxelCoord.Size, 3);
                debugColor = Color.green;
                break;
            case VoxelType.Water:
                Water = Mathf.Pow(VoxelCoord.Size, 3);
                debugColor = Color.blue;
                break;
            case VoxelType.Air:
                debugColor = Color.yellow;
                break;
        }

        if (EnvironmentService.Instance.ShowVoxels)
        {
            Debug.DrawRay(Coord.CenterTop, Vector3.down * VoxelCoord.Size, debugColor);
        }
    }

    private void UpdateVoxelType()
    {
        int landLayer = LayerMask.NameToLayer("Soil");
        int waterLayer = LayerMask.NameToLayer("Water");
        int layerMask = 1 << landLayer | 1 << waterLayer;
        Physics.Raycast(Coord.CenterTop, Vector3.down, out var hit, VoxelCoord.Size, layerMask);

        if (hit.transform?.gameObject?.layer == landLayer)
        {
            Type = VoxelType.Land;
        }
        else if (hit.transform?.gameObject?.layer == waterLayer)
        {
            Type = VoxelType.Water;
        }
        else
        {
            Type = VoxelType.Air;
        }
    }
}

public class VoxelCoord : IEquatable<VoxelCoord>
{
    public const float Size = 0.25f;

    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    public Vector3 Location => new Vector3(X, Y, Z) * Size;
    public Vector3 Center => Location + new Vector3(Size / 2, Size / 2, Size / 2);
    public Vector3 CenterTop => Location + new Vector3(Size / 2, Size, Size / 2);

    public VoxelCoord(Vector3 location)
    {
        X = Mathf.FloorToInt(location.x / Size);
        Y = Mathf.FloorToInt(location.y / Size);
        Z = Mathf.FloorToInt(location.z / Size);
    }

    public bool Equals(VoxelCoord other)
    {
        return X == other.X &&
               Y == other.Y &&
               Z == other.Z;
    }
}

public enum VoxelType
{
    Air,
    Land,
    Water,
}
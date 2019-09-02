using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;

public class VoxelService : MonoBehaviour
{
    private Dictionary<VoxelCoord, Voxel> _voxels;

    public VoxelService()
    {
        _voxels = new Dictionary<VoxelCoord, Voxel>();
    }


    public Voxel GetVoxel(VoxelCoord coord)
    {
        if (coord.Location.y > 50 || coord.Location.y < -50) return null;

        if (!_voxels.TryGetValue(coord, out var voxel))
        {
            voxel = new Voxel(coord);
            _voxels[coord] = voxel;
        }

        return voxel;
    }

    /*
    public void AddLight(VoxelCoord coord, float amount)
    {

    }
    public float RemoveLight(VoxelCoord coord, float requestedAmount)
    {

    }

    public void AddWater(VoxelCoord coord, float amount)
    {

    }
    public float RemoveWater(VoxelCoord coord, float requestedAmount)
    {

    }
    */

    public void AddSoil(VoxelCoord coord, float amount)
    {
        var voxel = GetVoxel(coord);
        if (voxel == null) return;

        voxel.Soil += amount;
    }
    public float RemoveSoil(VoxelCoord coord, float requestedAmount)
    {
        var voxel = GetVoxel(coord);
        if (voxel == null) return 0f;

        var amount = Mathf.Clamp(requestedAmount, 0, voxel.Soil);
        voxel.Soil -= amount;
        return amount;
    }
}

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
        Water = 0;
        Soil = 0;

        int landLayer = LayerMask.NameToLayer("Soil");
        int waterLayer = LayerMask.NameToLayer("Water");
        int layerMask = 1 << landLayer | 1 << waterLayer;
        Physics.Raycast(Coord.CenterTop, Vector3.down, out var hit, VoxelCoord.Size, layerMask);

        if (hit.transform?.gameObject?.layer == landLayer)
        {
            Type = VoxelType.Land;
            Soil = Mathf.Pow(VoxelCoord.Size, 3);
            Water = Mathf.Pow(VoxelCoord.Size / 4, 3);
            Debug.DrawRay(Coord.CenterTop, Vector3.down * VoxelCoord.Size, Color.green);
        }
        else if (hit.transform?.gameObject?.layer == waterLayer)
        {
            Type = VoxelType.Water;
            Water = Mathf.Pow(VoxelCoord.Size, 3);
            Debug.DrawRay(Coord.CenterTop, Vector3.down * VoxelCoord.Size, Color.blue);
        }
        else
        {
            Type = VoxelType.Air;
            Debug.DrawRay(Coord.CenterTop, Vector3.down * VoxelCoord.Size, Color.yellow);
        }
    }

    private bool IsStructureOccupyingVoxel(Structure structure)
    {
        return structure.Model.GetComponentInChildren<Collider>()?.bounds.Contains(Coord.Center) ?? false;
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

public enum VoxelType {
    Air,
    Land,
    Water,
}

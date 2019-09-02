using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;

public class VoxelSystem : MonoBehaviour
{
    private static Dictionary<VoxelCoord, Voxel> _voxels = new Dictionary<VoxelCoord, Voxel>();

    public static Voxel GetVoxel(VoxelCoord coord)
    {
        if (coord.Y > 20) return null;

        _voxels.TryGetValue(coord, out var voxel);
        if (voxel == null)
        {
            voxel = new Voxel(coord);
            _voxels[coord] = voxel;
        }
        return voxel;
    }
}

public class Voxel
{
    public const float Size = 1;

    public VoxelCoord Coord { get; }
    public Structure Occupant { get; private set; }
    public float LightPercentage { get; private set; }

    public Vector3 Center => new Vector3(Coord.X + Size/2, Coord.Y + Size / 2, Coord.Z + Size / 2);
    public float AbsoluteLight => Size * LightPercentage;

    private DateTime _lastUpdated;


    public Voxel(VoxelCoord coord)
    {
        Visualizer.MarkPosition(coord.ToVector3());
        Coord = coord;
        LightPercentage =
            1; //VoxelSystem.GetVoxel(new VoxelCoord(new Vector3(coord.X, coord.Y + 1, coord.Z)))?.LightPercentage ?? 1;
    }

    public List<Voxel> Fill(Structure structure, List<Voxel> filledVoxels = null)
    {
        return new List<Voxel>();
        if (filledVoxels == null)
            filledVoxels = new List<Voxel>();

        if (!filledVoxels.Contains(this) &&
            IsStructureOccupyingVoxel(structure))
        {
            Occupant = structure;
            filledVoxels.Add(this);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        var neighbor = VoxelSystem.GetVoxel(new VoxelCoord(new Vector3(Coord.X + x, Coord.Y + y, Coord.Z + z)));
                        if (!filledVoxels.Contains(neighbor))
                        {
                            neighbor.Fill(structure, filledVoxels);
                        }
                    }
                }
            }

            Update();
        }

        return filledVoxels;
    }

    public void Update()
    {
        return;
        if ((_lastUpdated - DateTime.Now).TotalSeconds < 1) return;
        else _lastUpdated = DateTime.Now;

        Occupant = IsStructureOccupyingVoxel(Occupant) ? Occupant : null;

        var top = VoxelSystem.GetVoxel(new VoxelCoord(new Vector3(Coord.X, Coord.Y + 1, Coord.Z)));
        top?.Update();

        var topLight = top?.LightPercentage ?? 1;
        var lightObsorbtion = Occupant == null ? 0 : 0.3f;
        LightPercentage = Mathf.Clamp(topLight - lightObsorbtion, 0, 1);
    }

    private bool IsStructureOccupyingVoxel(Structure structure)
    {
        return structure.Model.GetComponentInChildren<Collider>()?.bounds.Contains(Center) ?? false;
    }
}

public class VoxelCoord : IEquatable<VoxelCoord>
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    public VoxelCoord(Vector3 location)
    {
        location *= Voxel.Size;
        X = Mathf.RoundToInt(location.x);
        Y = Mathf.RoundToInt(location.y);
        Z = Mathf.RoundToInt(location.z);
    }

    public bool Equals(VoxelCoord other)
    {
        return X == other.X &&
               Y == other.Y &&
               Z == other.Z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X,Y,Z);
    }
}
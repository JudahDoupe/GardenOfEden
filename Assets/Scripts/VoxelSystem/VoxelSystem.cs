using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelSystem : MonoBehaviour
{
    private static Dictionary<VoxelCoord, Voxel> _voxels = new Dictionary<VoxelCoord, Voxel>();

    public static Voxel GetVoxel(VoxelCoord coord)
    {
        if (coord.Y > 100) return null;

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
    public readonly VoxelCoord Coord;
    public Vector3 Center => new Vector3(Coord.X + Size/2, Coord.Y + Size / 2, Coord.Z + Size / 2);

    public float Light { get; private set; }

    public Voxel(VoxelCoord coord)
    {
        Coord = coord;
        Light = VoxelSystem.GetVoxel(new VoxelCoord(new Vector3(coord.X, coord.Y + 1, coord.Z)))?.Light ?? 1;
    }

    public void Update(GameObject occupant)
    {
        var collider = occupant.GetComponent<Collider>();
        if (collider?.bounds.Contains(Center) ?? false)
        {
            Light = 0;
            //Figure out a way to feed this light to the leaf
            UpdateNeighbors(occupant);
        }
    }

    public void UpdateNeighbors(GameObject occupant)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    var neighborCoord = new VoxelCoord(new Vector3(Coord.X + x, Coord.Y + y, Coord.Z + z));
                    if (neighborCoord != Coord)
                    {
                        VoxelSystem.GetVoxel(neighborCoord)?.Update(occupant);
                    }
                }
            }
        }
    }

    public void UpdateDown(GameObject occupant)
    {
        var neighborCoord = new VoxelCoord(new Vector3(Coord.X, Coord.Y - 1, Coord.Z));
        Update(occupant);
        VoxelSystem.GetVoxel(neighborCoord)?.UpdateDown(occupant);
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
}
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

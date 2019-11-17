﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentService : MonoBehaviour
{
    public static float GetDate()
    {
        return Instance._date;
    }

    /*
    public static float GetLight(Vector3 location)
    {

    }
    public static float AbsorbLight(Vector3 location, float requestedAmount)
    {

    }
    */

    public static UnitsOfWater SampleWater(Vector3 location)
    {
        return _waterShaderService.SampleWater(location);
    }
    public static UnitsOfWater AbsorbWater(Texture2D rootMap, Vector3 location, float deltaTimeInDays)
    {
        var waterMap = _waterShaderService.AbsorbWater(rootMap, deltaTimeInDays / 10);
        var xy = ComputeShaderUtils.LocationToXy(location);
        var summedWaterDepth = waterMap.GetPixels(Mathf.FloorToInt(xy.x - 15), Mathf.FloorToInt(xy.y - 15), 30, 30)
            .Sum(color => color.r + color.g + color.b);
        return UnitsOfWater.FromPixel(summedWaterDepth);
    }

    public static float SampleSoil(Vector3 location)
    {
        var coord = new VoxelCoord(location);
        var voxel = Instance.GetVoxel(coord);
        return voxel.Soil;
    }
    public static float AbsorbSoil(Vector3 location, float requestedAmount)
    {
        var coord = new VoxelCoord(location);
        var voxel = Instance.GetVoxel(coord);
        if (voxel == null) return 0f;

        var amount = Mathf.Clamp(requestedAmount, 0, voxel.Soil);
        voxel.Soil -= amount;
        return amount;
    }
    public static void AddSoil(Vector3 location, float amount)
    {
        var coord = new VoxelCoord(location);
        var voxel = Instance.GetVoxel(coord);
        if (voxel == null) return;

        voxel.Soil += amount;
    }

    /* INNER MECHINATIONS */

    public static EnvironmentService Instance;
    private static SoilService _soilService;
    private static WaterService _waterShaderService;
    private Dictionary<VoxelCoord, Voxel> _voxels;

    private float _date;

    private void Awake()
    {
        Instance = this;
        _soilService = GetComponent<SoilService>();
        _waterShaderService = GetComponent<WaterService>();
        _voxels = new Dictionary<VoxelCoord, Voxel>();
        _date = 0;
    }

    private void Update()
    {
        _date += Time.deltaTime;
    }

    private Voxel GetVoxel(VoxelCoord coord)
    {
        if (coord.Location.y > 50 || coord.Location.y < -50) return null;

        if (!_voxels.TryGetValue(coord, out var voxel))
        {
            voxel = new Voxel(coord);
            _voxels[coord] = voxel;
        }

        return voxel;
    }
}

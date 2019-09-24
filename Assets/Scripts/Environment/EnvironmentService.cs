using System.Collections.Generic;
using UnityEngine;

public class EnvironmentService : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool ShowVoxels = false;

    public Camera TerrainCamera;

    /* API */

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

    public static float GetWater(Vector3 location)
    {
        var waterMap = ComputeShaderService.GetWaterMap();
        var uv = ComputeShaderService.LocationToUV(location);
        var color = waterMap.GetPixelBilinear(uv.x, uv.y);

        return Mathf.Clamp(color.r + color.g + color.b, 0, 1);
    }


    public static float GetSoil(Vector3 location)
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
    private Dictionary<VoxelCoord, Voxel> _voxels;

    private float _date;

    void Awake()
    {
        Instance = this;
        _voxels = new Dictionary<VoxelCoord, Voxel>();
        _date = 0;
    }

    void Update()
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

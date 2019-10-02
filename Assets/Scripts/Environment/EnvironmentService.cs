using System.Collections.Generic;
using System.Linq;
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

    public static float SampleWater(Vector3 location)
    {
        var waterMap = ComputeShaderService.RenderTextureToTexture2D(ComputeShaderService.Instance.WaterMap);
        var uv = ComputeShaderService.LocationToUV(location);
        var color = waterMap.GetPixelBilinear(uv.x, uv.y);

        return Mathf.Clamp(color.r + color.g + color.b, 0, 1);
    }
    public static float AbsorbWater(Texture2D rootMap, Vector3 location, float deltaTimeInDays)
    {
        var waterMap = ComputeShaderService.AbsorbWater(rootMap, deltaTimeInDays / 10);
        var uv = ComputeShaderService.LocationToUV(location);
        var x = Mathf.FloorToInt(uv.x * 512);
        var y = Mathf.FloorToInt(uv.y * 512);

        return waterMap.GetPixels(x - 15, y - 15, 30, 30).Sum(color => color.r + color.g + color.b);
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

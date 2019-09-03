using UnityEngine;

public class EnvironmentAPI : MonoBehaviour
{
    public static EnvironmentAPI Instance;

    public bool ShowVoxels = false;

    private VoxelService _voxelService;

    void Awake()
    {
        Instance = this;
        _voxelService = new VoxelService();
    }

    /*
    public static float GetLight(Vector3 location)
    {

    }
    public static float GetWater(Vector3 location)
    {

    }

    public static float AbsorbLight(Vector3 location, float requestedAmount)
    {

    }
    public static float AbsorbWater(Vector3 location, float requestedAmount)
    {

    }
    */

    public static float GetSoil(Vector3 location)
    {
        var coord = new VoxelCoord(location);
        var voxel = Instance._voxelService.GetVoxel(coord);
        return voxel.Soil;
    }
    public static float AbsorbSoil(Vector3 location, float requestedAmount)
    {
        var coord = new VoxelCoord(location);
        var amount = Instance._voxelService.RemoveSoil(coord, requestedAmount);
        return amount;
    }
    public static void AddSoil(Vector3 location, float amount)
    {
        var coord = new VoxelCoord(location);
        Instance._voxelService.AddSoil(coord, amount);
    }
}

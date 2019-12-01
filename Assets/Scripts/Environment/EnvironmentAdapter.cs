using System.Linq;
using UnityEngine;

public class EnvironmentAdapter : MonoBehaviour
{
    public static float GetDate()
    {
        return Instance._date;
    }


    public static float SampleLight(Vector3 location)
    {
        return 1;
    }
    public static float SampleWaterDepth(Vector3 location)
    {
        return _waterService.SampleWaterDepth(location) 
               + _soilService.SampleWaterDepth(location);
    }
    public static float SampleSoilDepth(Vector3 location)
    {
        return _soilService.SampleSoilDepth(location);
    }
    public static float SampleRootDepth(Vector3 location)
    {
        return _soilService.SampleRootDepth(location);
    }

    public static UnitsOfWater AbsorbWater(Texture2D rootMap, Vector3 location, float deltaTimeInDays)
    {
        var waterMap = _soilService.AbsorbWater(rootMap, deltaTimeInDays / 10);
        var xy = ComputeShaderUtils.LocationToXy(location);
        var summedWaterDepth = waterMap.GetPixels(Mathf.FloorToInt(xy.x - 15), Mathf.FloorToInt(xy.y - 15), 30, 30)
            .Sum(color => color.r + color.g + color.b);
        return UnitsOfWater.FromPixel(summedWaterDepth);
    }

    public static Texture2D SpreadRoots(Texture2D currentRoots, Vector3 location, float radius, float depth)
    {
        return _soilService.SpreadRoots(currentRoots, location, radius, depth);
    }

    /* INNER MECHINATIONS */

    public static EnvironmentAdapter Instance;
    private static SoilService _soilService;
    private static WaterService _waterService;

    private float _date;

    private void Awake()
    {
        Instance = this;
        _soilService = GetComponent<SoilService>();
        _waterService = GetComponent<WaterService>();
        _date = 0;
    }

    private void Update()
    {
        _date += Time.deltaTime;
    }
}

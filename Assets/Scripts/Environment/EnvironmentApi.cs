using UnityEngine;

public class EnvironmentApi : MonoBehaviour
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

    /* INNER MECHINATIONS */

    public static EnvironmentApi Instance;
    private static SoilService _soilService;
    private static WaterService _waterService;
    private static RootService _rootService;

    private float _date;

    private void Awake()
    {
        Instance = this;
        _soilService = GetComponent<SoilService>();
        _waterService = GetComponent<WaterService>();
        _rootService = GetComponent<RootService>();
        _date = 0;
    }

    private void Update()
    {
        _date += Time.deltaTime;
    }
}

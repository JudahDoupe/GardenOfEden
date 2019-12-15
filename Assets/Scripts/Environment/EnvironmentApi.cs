using UnityEngine;

public class EnvironmentApi : MonoBehaviour
{
    public static float GetDate()
    {
        return Instance._date;
    }


    public static Area GetAbsorpedLight(int Id)
    {
        return _lightService.GetAbsorpedLight(Id);
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
    private static LightService _lightService;

    private float _date;

    private void Awake()
    {
        Instance = this;
        _soilService = GetComponent<SoilService>();
        _waterService = GetComponent<WaterService>();
        _lightService = GetComponent<LightService>();
        _date = 0;
    }

    private void Update()
    {
        _date += Time.deltaTime;
    }
}

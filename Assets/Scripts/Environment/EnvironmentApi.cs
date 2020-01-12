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
               + _landService.SampleWaterDepth(location);
    }
    public static float SampleSoilDepth(Vector3 location)
    {
        return _landService.SampleSoilDepth(location);
    }

    /* INNER MECHINATIONS */

    public static EnvironmentApi Instance;
    private static LandService _landService;
    private static WaterService _waterService;
    private static LightService _lightService;

    private float _date;

    private void Awake()
    {
        Instance = this;
        _landService = GetComponent<LandService>();
        _waterService = GetComponent<WaterService>();
        _lightService = GetComponent<LightService>();
        _date = 0;
    }

    private void Update()
    {
        _date += Time.deltaTime;
    }
}

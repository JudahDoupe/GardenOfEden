using UnityEngine;

public class DI : MonoBehaviour
{
    public static LandService LandService;
    public static LightService LightService;
    public static WaterService WaterService;

    public static GrowthService GrowthService;
    public static RootService RootService;

    public static GameService GameService;

    public static CameraController CameraController;
    public static UIController UIController;

    private void Start()
    {
        LandService = FindObjectOfType<LandService>();
        LightService = FindObjectOfType<LightService>();
        WaterService = FindObjectOfType<WaterService>();

        GrowthService = FindObjectOfType<GrowthService>();
        RootService = FindObjectOfType<RootService>();

        GameService = FindObjectOfType<GameService>();

        CameraController = FindObjectOfType<CameraController>();
        UIController = FindObjectOfType<UIController>();
    }
}

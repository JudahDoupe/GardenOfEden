using UnityEngine;

public class DI : MonoBehaviour
{
    public static LandService LandService;
    public static LightService LightService;
    public static WaterService WaterService;

    public static ReproductionService ReproductionService;
    public static RootService RootService;
    public static PlantGrowthService PlantGrowthService;

    public static GameService GameService;

    public static CameraController CameraController;
    public static UIController UIController;

    private void Start()
    {
        LandService = FindObjectOfType<LandService>();
        LightService = FindObjectOfType<LightService>();
        WaterService = FindObjectOfType<WaterService>();

        ReproductionService = FindObjectOfType<ReproductionService>();
        RootService = FindObjectOfType<RootService>();
        PlantGrowthService = FindObjectOfType<PlantGrowthService>();

        GameService = FindObjectOfType<GameService>();

        CameraController = FindObjectOfType<CameraController>();
        UIController = FindObjectOfType<UIController>();
    }
}

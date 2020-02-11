using UnityEngine;

public class DI : MonoBehaviour
{
    public static LandService LandService;
    public static LightService LightService;
    public static WaterService WaterService;

    public static GrowthService GrowthService;

    public static GameService GameService;

    public static CameraController CameraController;
    public static CameraFocus CameraFocus;
    public static CameraTransform CameraTransform;
    public static CameraPostProcessor CameraPostProcessor;

    private void Start()
    {
        LandService = FindObjectOfType<LandService>();
        LightService = FindObjectOfType<LightService>();
        WaterService = FindObjectOfType<WaterService>();

        GrowthService = FindObjectOfType<GrowthService>();

        GameService = FindObjectOfType<GameService>();

        CameraController = FindObjectOfType<CameraController>();
        CameraFocus = FindObjectOfType<CameraFocus>();
        CameraTransform = FindObjectOfType<CameraTransform>();
        CameraPostProcessor = FindObjectOfType<CameraPostProcessor>();
    }
}

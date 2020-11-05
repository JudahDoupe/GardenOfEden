using Assets.Scripts.Plants.ECS;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static ILandService LandService;
    public static WaterService WaterService;

    public static GameService GameService;
    public static TimeService TimeService;
    public static WorldDataStore WorldService;

    public static CameraController CameraController;
    public static RenderMeshLibrary RenderMeshLibrary;


    private void Awake()
    {
        LandService = FindObjectOfType<LandService>();
        WaterService = FindObjectOfType<WaterService>();

        GameService = FindObjectOfType<GameService>();
        TimeService = FindObjectOfType<TimeService>();
        WorldService = FindObjectOfType<WorldDataStore>();

        CameraController = FindObjectOfType<CameraController>();
        RenderMeshLibrary = FindObjectOfType<RenderMeshLibrary>();
    }
}

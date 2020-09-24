using Assets.Scripts.Plants.ECS;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static LandService LandService;
    public static LightService LightService;
    public static WaterService WaterService;

    public static GrowthService GrowthService;
    public static PlantSearchService PlantSearchService;

    public static GameService GameService;
    public static TimeService TimeService;
    public static WorldDataStore WorldService;

    public static CameraController CameraController;
    public static RenderMeshLibrary RenderMeshLibrary;
    public static ArchetypeLibrary ArchetypeLibrary;


    private void Awake()
    {
        LandService = FindObjectOfType<LandService>();
        LightService = FindObjectOfType<LightService>();
        WaterService = FindObjectOfType<WaterService>();

        GrowthService = FindObjectOfType<GrowthService>();
        PlantSearchService = FindObjectOfType<PlantSearchService>();

        GameService = FindObjectOfType<GameService>();
        TimeService = FindObjectOfType<TimeService>();
        WorldService = FindObjectOfType<WorldDataStore>();

        CameraController = FindObjectOfType<CameraController>();
        RenderMeshLibrary = FindObjectOfType<RenderMeshLibrary>();
        ArchetypeLibrary = FindObjectOfType<ArchetypeLibrary>();
    }
}

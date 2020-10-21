using Assets.Scripts.Plants.ECS.Components;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Assets.Scripts.Plants.ECS.Services;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class GameService : MonoBehaviour
{
    public bool IsGameInProgress { get; private set; }

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        IsGameInProgress = true;

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (var i = 0; i < 500; i++)
        {
            var baseNode = em.CreateEntity();
            em.AddComponentData(baseNode, new Assets.Scripts.Plants.ECS.Components.Node{Size = new float3(0.5f,0.5f,0.5f)});
            em.AddComponentData(baseNode, new Translation { Value = Singleton.LandService.ClampToTerrain(new Vector3(Random.Range(-100f, 100f), 50, Random.Range(-200f, 0f))) });
            em.AddComponentData(baseNode, new Rotation { Value = Quaternion.LookRotation(Vector3.up) });
            em.AddComponentData(baseNode, new LocalToWorld());
            em.AddComponentData(baseNode, new EnergyStore {Capacity = 1, Quantity = 1});
            em.AddComponentData(baseNode, new EnergyFlow());
            em.AddComponentData(baseNode, new LightAbsorption ());

            var topNode = em.CreateEntity();
            em.AddComponentData(topNode, new Assets.Scripts.Plants.ECS.Components.Node());
            em.AddComponentData(topNode, new Internode());
            em.AddComponentData(topNode, new Translation());
            em.AddComponentData(topNode, new Rotation { Value = Quaternion.LookRotation(Vector3.forward) });
            em.AddComponentData(topNode, new Parent {Value = baseNode});
            em.AddComponentData(topNode, new LocalToParent());
            em.AddComponentData(topNode, new LocalToWorld());
            em.AddComponentData(topNode, new EnergyStore());
            em.AddComponentData(topNode, new EnergyFlow());
            em.AddComponentData(topNode, new LightAbsorption ());
            em.AddComponentData(topNode, new Photosynthesis { Efficiency = 1 });
            em.AddComponentData(topNode, new AssignInternodeMesh { MeshName = "GreenStem" });
            em.AddComponentData(topNode, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 1, InternodeRadius = 0.1f });

            var leafNode = em.CreateEntity();
            em.AddComponentData(leafNode, new Assets.Scripts.Plants.ECS.Components.Node());
            em.AddComponentData(leafNode, new Internode());
            em.AddComponentData(leafNode, new Translation( ));
            em.AddComponentData(leafNode, new Rotation { Value = Quaternion.LookRotation(Vector3.left, Vector3.down) });
            em.AddComponentData(leafNode, new Parent { Value = topNode });
            em.AddComponentData(leafNode, new LocalToParent());
            em.AddComponentData(leafNode, new LocalToWorld());
            em.AddComponentData(leafNode, new EnergyStore());
            em.AddComponentData(leafNode, new EnergyFlow());
            em.AddComponentData(leafNode, new LightAbsorption ());
            em.AddComponentData(leafNode, new Photosynthesis { Efficiency = 1});
            em.AddComponentData(leafNode, new AssignInternodeMesh { MeshName = "GreenStem"});
            em.AddComponentData(leafNode, new AssignNodeMesh { MeshName = "Leaf"});
            em.AddComponentData(leafNode, new PrimaryGrowth { GrowthRate = 0.1f, InternodeLength = 0.1f, InternodeRadius = 0.1f, NodeSize = 1});

            var budNode = em.CreateEntity();
            em.AddComponentData(budNode, new Assets.Scripts.Plants.ECS.Components.Node());
            em.AddComponentData(budNode, new Internode());
            em.AddComponentData(budNode, new Translation());
            em.AddComponentData(budNode, new Rotation { Value = Quaternion.LookRotation(Vector3.forward) });
            em.AddComponentData(budNode, new Parent {Value = topNode});
            em.AddComponentData(budNode, new LocalToParent());
            em.AddComponentData(budNode, new LocalToWorld());
            em.AddComponentData(budNode, new EnergyStore());
            em.AddComponentData(budNode, new EnergyFlow());
            em.AddComponentData(budNode, new LightAbsorption ());
            em.AddComponentData(budNode, new TerminalBud());
            em.AddComponentData(budNode, new AssignInternodeMesh { MeshName = "GreenStem" });
        }

    }

    private void EndGame()
    {
        IsGameInProgress = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

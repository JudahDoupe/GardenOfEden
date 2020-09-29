using Assets.Scripts.Plants.ECS.Components;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

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
            var plant = em.CreateEntity(Singleton.ArchetypeLibrary.Archetypes["Plant"]);
            em.SetName(plant, "plant");
            em.SetComponentData(plant, new Translation { Value = Singleton.LandService.ClampToTerrain(new Vector3(UnityEngine.Random.Range(-100f,100f), 50, UnityEngine.Random.Range(-200f, 0f))) });
            em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.up) });

            var lastNode = plant;
            for (var j = 0; j < 5; j++)
            {

                var angle = UnityEngine.Random.Range(-0.1f, 0.1f);
                var offset = new Vector3(angle, angle, angle);

                var node = em.CreateEntity(Singleton.ArchetypeLibrary.Archetypes["Node"]);
                em.SetName(node, "node");
                em.SetComponentData(node, new Translation { Value = new Vector3(0, 0, 0.1f) });
                em.SetComponentData(node, new Rotation { Value = Quaternion.LookRotation(Vector3.forward + offset) });
                em.SetComponentData(node, new Parent { Value = lastNode });

                var internode = em.CreateEntity(Singleton.ArchetypeLibrary.Archetypes["Internode"]);
                em.SetName(internode, "internodeMesh");
                em.SetComponentData(internode, new Rotation { Value = Quaternion.LookRotation(Vector3.forward) });
                em.SetComponentData(internode, new NonUniformScale { Value = new Vector3(0.1f, 0.1f, 1) });
                em.SetComponentData(internode, new Internode { HeadNode = node, TailNode = lastNode });

                em.SetComponentData(node, new InternodeReference { Internode = internode });
                lastNode = node;
            }

            em.AddComponent<TerminalBud>(lastNode);

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

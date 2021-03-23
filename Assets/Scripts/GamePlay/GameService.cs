using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Dna;
using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using Assets.Scripts.Plants.Setup;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Rendering;

public class GameService : MonoBehaviour
{

    public static EntityArchetype plantNodeArchetype;
    private EntityManager em;

    private void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        plantNodeArchetype = em.CreateArchetype(
            typeof(Node),
            typeof(Translation),
            typeof(Rotation),
            typeof(Parent),
            typeof(LocalToParent),
            typeof(LocalToWorld),
            typeof(EnergyStore),
            typeof(EnergyFlow),
            typeof(LightBlocker),
            typeof(Dormant),
            typeof(UpdateChunk),
            typeof(DnaReference),
            typeof(Metabolism),
            typeof(Health)
        );

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

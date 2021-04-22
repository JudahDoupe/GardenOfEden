using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Dna;
using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using Assets.Scripts.Plants.Setup;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class DnaService : MonoBehaviour
{
    public static EntityArchetype PlantNodeArchetype;
    public static GeneLibrary GeneLibrary;

    private static Dictionary<int, Dna> DnaLibrary = new Dictionary<int, Dna>();


    void Start()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        GeneLibrary = new GeneLibrary();
        PlantNodeArchetype = em.CreateArchetype(
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
            typeof(Metabolism),
            typeof(Health)
        );
    }

    public static int RegisterNewSpecies(Dna dna)
    {
        var speciesId = DnaLibrary.Any() ? DnaLibrary.Keys.Max() + 1 : 0;
        DnaLibrary[speciesId] = dna;
        return speciesId;
    }

    public static Dna GetSpeciesDna(int speciesId)
    {
        return DnaLibrary[speciesId];
    }
}

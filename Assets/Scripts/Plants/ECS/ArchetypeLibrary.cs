using Assets.Scripts.Plants.ECS.Components;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.ECS
{
    public class ArchetypeLibrary : MonoBehaviour
    {
        public Dictionary<string, EntityArchetype> Archetypes = new Dictionary<string, EntityArchetype>();

        public void Start()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            Archetypes["Plant"] = em.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld));
            Archetypes["Node"] = em.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Parent),
                typeof(InternodeReference),
                typeof(LocalToWorld),
                typeof(LocalToParent));
            Archetypes["Internode"] = em.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(LocalToWorld),
                typeof(Internode),
                typeof(RenderMesh),
                typeof(RenderBounds));
        }
    }
}

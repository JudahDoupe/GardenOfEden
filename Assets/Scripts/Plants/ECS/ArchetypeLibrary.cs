﻿using Assets.Scripts.Plants.ECS.Components;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.ECS
{
    public class ArchetypeLibrary : MonoBehaviour
    {
        public Dictionary<string, EntityArchetype> Library = new Dictionary<string, EntityArchetype>();

        public void Start()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            Library["Plant"] = em.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld));
            Library["Internode"] = em.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(LocalToWorld),
                typeof(Internode));
            Library["Node"] = em.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Parent),
                typeof(InternodeReference),
                typeof(LocalToWorld),
                typeof(LocalToParent),
                typeof(EnergyStore));
        }
    }
}

﻿using System;
using System.Collections.Generic;
using Assets.Plants.Systems.Cleanup;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class RenderMeshLibrary : MonoBehaviour
    {
        public List<MeshContainer> Meshes = new List<MeshContainer>();
        public Dictionary<string, MeshContainer> Library = new Dictionary<string, MeshContainer>();

        public void Start()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var meshArchetype = em.CreateArchetype(
                typeof(RenderMesh),
                typeof(RenderBounds),
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(LocalToParent),
                typeof(LocalToWorld),
                typeof(Parent),
                typeof(NodeMesh)
            );

            foreach (var mesh in Meshes)
            {
                mesh.Desc = new RenderMeshDescription(
                    mesh.Mesh.mesh,
                    mesh.Mesh.material,
                    mesh.Mesh.castShadows,
                    mesh.Mesh.receiveShadows);
                mesh.Entity = em.CreateEntity(meshArchetype);
                em.SetName(mesh.Entity, $"Mesh_{mesh.Name}");
                RenderMeshUtility.AddComponents(mesh.Entity, em, mesh.Desc);

                Library.Add(mesh.Name, mesh);

            }
        }

        [Serializable]
        public class MeshContainer
        {
            public string Name;
            public RenderMesh Mesh;
            public RenderMeshDescription Desc;
            public Entity Entity;
        }
    }
}
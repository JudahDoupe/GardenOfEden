using System;
using System.Collections.Generic;
using Assets.Scripts.Plants.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.ECS
{
    public class RenderMeshLibrary : MonoBehaviour
    {
        public List<MeshContainer> Meshes = new List<MeshContainer>();
        public Dictionary<string, MeshContainer> Library = new Dictionary<string, MeshContainer>();

        public void Start()
        {
            foreach (var mesh in Meshes)
            {
                mesh.Bounds = new RenderBounds()
                {
                    Value = new AABB()
                    {
                        Center = new float3(mesh.Mesh.mesh.bounds.center.x, mesh.Mesh.mesh.bounds.center.y, mesh.Mesh.mesh.bounds.center.z),
                        Extents = new float3(mesh.Mesh.mesh.bounds.extents.x, mesh.Mesh.mesh.bounds.extents.y, mesh.Mesh.mesh.bounds.extents.z)
                    }
                };

                Library.Add(mesh.Name, mesh);
            }
        }

        [Serializable]
        public class MeshContainer
        {
            public string Name;
            public RenderMesh Mesh;
            public RenderBounds Bounds;
        }
    }
}

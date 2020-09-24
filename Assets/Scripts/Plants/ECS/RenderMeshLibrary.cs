using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Assets.Scripts.Plants.ECS
{
    public class RenderMeshLibrary : MonoBehaviour
    {
        public List<MeshContainer> Meshes = new List<MeshContainer>();

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

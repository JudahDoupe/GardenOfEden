using System;
using System.Collections.Generic;
using Unity.Rendering;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class RenderMeshLibrary : MonoBehaviour
    {
        public List<MeshContainer> Meshes = new List<MeshContainer>();
        public Dictionary<string, MeshContainer> Library = new Dictionary<string, MeshContainer>();

        public void Start()
        {
            foreach (var mesh in Meshes)
            {
                mesh.Desc = new RenderMeshDescription(
                    mesh.Mesh.mesh,
                    mesh.Mesh.material,
                    mesh.Mesh.castShadows,
                    mesh.Mesh.receiveShadows);

                Library.Add(mesh.Name, mesh);
            }
        }

        [Serializable]
        public class MeshContainer
        {
            public string Name;
            public RenderMesh Mesh;
            public RenderMeshDescription Desc;
        }
    }
}

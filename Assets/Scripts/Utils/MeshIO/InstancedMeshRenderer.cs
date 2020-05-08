using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InstancedMeshRenderer : MonoBehaviour
{
    public List<RenderingData> Meshes;
    private static readonly Dictionary<string, Tuple<Mesh,Material>> MeshCache = new Dictionary<string, Tuple<Mesh, Material>>();


    private static readonly Dictionary<string, List<RenderingInstanceData>> Instances = new Dictionary<string, List<RenderingInstanceData>>();
    public static RenderingInstanceData AddInstance(string meshId)
    {
        var instance = new RenderingInstanceData(meshId);
        List<RenderingInstanceData> instances; 
        if(Instances.TryGetValue(instance.MeshId, out instances))
        {
            instances.Add(instance);
        }
        else
        {
            Debug.LogError($"No mesh registered with id: {instance.MeshId}");
        }

        return instance;
    }
    public static void RemoveInstance(RenderingInstanceData instance)
    {
        List<RenderingInstanceData> instances;
        if (Instances.TryGetValue(instance.MeshId, out instances))
        {
            instances.Remove(instance);
        }
        else
        {
            Debug.LogError($"No mesh registered with id: {instance.MeshId}");
        }
    }


    private void Awake()
    {
        foreach(var mesh in Meshes)
        {
            MeshCache.Add(mesh.MeshId, Tuple.Create(mesh.Mesh, mesh.Material));
            Instances.Add(mesh.MeshId, new List<RenderingInstanceData>());
        }
    }
    private void Update()
    {
        foreach(var meshData in MeshCache)
        {
            var meshId = meshData.Key;
            var mesh = meshData.Value.Item1;
            var mat = meshData.Value.Item2;
            var instances = Instances[meshId];

            var batchsize = 1023;
            for (int i = 0; i < instances.Count(); i += batchsize)
            {
                batchsize = Math.Min(instances.Count() - i, batchsize);
                var batch = instances.GetRange(i, batchsize);
                RenderBatch(mesh, mat, batch);
            }
        }
    }
    private void RenderBatch(Mesh mesh, Material mat, List<RenderingInstanceData> instances)
    {
        Graphics.DrawMeshInstanced(mesh, 0, mat, instances.Select(i => i.Matrix).ToArray());
    }
}

public class RenderingInstanceData
{
    public string MeshId;
    public Matrix4x4 Matrix;

    public RenderingInstanceData(string meshId)
    {
        MeshId = meshId;
        Matrix = Matrix4x4.identity;
    }
}

[Serializable]
public class RenderingData
{
    public string MeshId;
    public Mesh Mesh;
    public Material Material;
}
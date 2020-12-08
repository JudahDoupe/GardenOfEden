using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Plants.Systems;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class EnvironmentalChunkService : MonoBehaviour
{
    public static int WorldChunkWidth = 3;
    public static float ChunkSize = 400;
    public static int TextureSize = 512;

    public GameObject ChunkPrefab;

    private Dictionary<int, EnvironmentalChunk> Chunks = new Dictionary<int, EnvironmentalChunk>();

    void Start()
    {
        for (int i = 0; i < WorldChunkWidth * WorldChunkWidth; i++)
        {
            CreateChunk(i);
        }
    }

    public EnvironmentalChunk GetChunk(float3 location)
    {
        return Chunks[LocationToChunkId(location)];
    }
    public List<EnvironmentalChunk> GetAllChunks()
    {
        return Chunks.Values.ToList();
    }

    public static int LocationToChunkId(float3 location)
    {
        var xy = math.int2(location.xz / ChunkSize);
        return xy.y * WorldChunkWidth + xy.x;
    }
    public static float2 LocationToUv(float3 location)
    {
        var modLoc = location.xz % ChunkSize;
        var positiveLoc = modLoc + new float2(ChunkSize);
        var clampedLoc = positiveLoc % ChunkSize;
        var uv = clampedLoc / ChunkSize;
        return uv;
    }
    public static float2 LocationToNormalizedUv(float3 location)
    {
        var uv = LocationToUv(location);
        return (uv + new float2(1, 1)) / 2;
    }
    public static float2 LocationToXy(float3 location)
    {
        var uv = LocationToUv(location);
        return uv * TextureSize;
    }
    public static int LocationToTextureIndex(float3 location)
    {
        var xy = math.int2(math.floor(LocationToXy(location)));
        var i = xy.y * TextureSize + xy.x;
        return i;
    }


    private void CreateChunk(int id)
    {
        var obj = Instantiate(ChunkPrefab);
        obj.transform.position = new Vector3((id % WorldChunkWidth) * ChunkSize, 0, (id / WorldChunkWidth) * ChunkSize);
        obj.name = $"Environment Chunk {id}";

        Chunks[id] = new EnvironmentalChunk
        {
            Id = id,
            Location = obj.transform.position,
            GameObject = obj,
            WaterMap = CreateRT(),
            WaterSourceMap = CreateRT(),
            LandMap = CreateRT(),
            SoilWaterMap = CreateRT(),
        };

        /*
        Chunks[id].WaterMap.LoadFromFile($"Map/Hills/water.tex");
        Chunks[id].WaterSourceMap.LoadFromFile($"Map/Hills/waterSource.tex", TextureFormat.RFloat);
        Chunks[id].LandMap.LoadFromFile($"Map/Hills/land.tex");
        Chunks[id].SoilWaterMap.LoadFromFile($"Map/Hills/soilWater.tex");
        */

        var landMaterial = obj.transform.Find("Land").GetComponent<MeshRenderer>().material;
        landMaterial.SetTexture("_LandMap", Chunks[id].LandMap);
        landMaterial.SetTexture("_SoilWaterMap", Chunks[id].SoilWaterMap);

        var waterMaterial = obj.transform.Find("Water").GetComponent<MeshRenderer>().material;
        waterMaterial.SetTexture("_WaterMap", Chunks[id].WaterMap);
    }

    private RenderTexture CreateRT()
    {
        var rtn = new RenderTexture(TextureSize, TextureSize, 4, GraphicsFormat.R32G32B32A32_SFloat);
        rtn.ResetTexture();
        return rtn;
    }
}

public class EnvironmentalChunk
{
    public int Id;
    public float3 Location;
    public GameObject GameObject;
    public RenderTexture WaterSourceMap;
    public RenderTexture WaterMap;
    public RenderTexture LandMap;
    public RenderTexture SoilWaterMap;
}

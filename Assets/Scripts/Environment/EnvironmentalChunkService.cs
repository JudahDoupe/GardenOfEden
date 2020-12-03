using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Plants.Systems;
using Unity.Mathematics;
using UnityEngine;

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

        Chunks[id] = new EnvironmentalChunk
        {
            Id = id,
            GameObject = obj,
            //WaterMap = "",
            //LandMap = ""
            //SoilWaterMap = ""
        };
    }
}

public class EnvironmentalChunk
{
    public int Id;
    public GameObject GameObject;
    public RenderTexture WaterSourceMap;
    public RenderTexture WaterMap;
    public RenderTexture LandMap;
    public RenderTexture SoilWaterMap;
}

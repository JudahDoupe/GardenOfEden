using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using LiteDB;

public class EnvironmentDataStore : MonoBehaviour
{
    public static RenderTexture WaterSourceMap { get; set; }
    public static RenderTexture WaterMap { get; set; }
    public static RenderTexture LandHeightMap { get; set; }
    public static RenderTexture PlateThicknessMaps { get; set; }
    public static RenderTexture TmpPlateThicknessMaps { get; set; }
    public static RenderTexture ContinentalIdMap { get; set; }
    public static RenderTexture TmpContinentalIdMap { get; set; }
    
    void Awake()
    {
        WaterMap = NewTexture(4, 6);
        WaterSourceMap = NewTexture(4, 6);
        LandHeightMap = NewTexture(1, 6);
        PlateThicknessMaps = NewTexture(1, 1);
        TmpPlateThicknessMaps = NewTexture(1, 1);
        ContinentalIdMap = NewTexture(1, 6);
        TmpContinentalIdMap = NewTexture(1, 6);

        Load();
    }

    public static void Load()
    {
        using var db = new LiteDatabase($@"{Application.persistentDataPath}\Environment.db");

        PlateThicknessMaps.CachedTextures();
        var col = db.GetCollection<EnvironementMapDto>("Maps");
        col.EnsureIndex(x => x.Name);

        col.FindById("WaterMap")?.LoadMap(WaterMap);
        col.FindById("WaterSourceMap")?.LoadMap(WaterSourceMap);
        col.FindById("LandHeightMap")?.LoadMap(LandHeightMap);
        col.FindById("PlateThicknessMaps")?.LoadMap(PlateThicknessMaps);
        col.FindById("ContinentalIdMap")?.LoadMap(ContinentalIdMap);

    }

    public static void Save()
    {
        using var db = new LiteDatabase($@"{Application.persistentDataPath}\Environment.db");

        PlateThicknessMaps.CachedTextures();
        var col = db.GetCollection<EnvironementMapDto>("Maps");
        col.EnsureIndex(x => x.Name);

        var maps = new List<EnvironementMapDto>()
        {
            new EnvironementMapDto("WaterSourceMap", WaterSourceMap),
            new EnvironementMapDto("WaterMap", WaterMap),
            new EnvironementMapDto("LandHeightMap", LandHeightMap),
            new EnvironementMapDto("PlateThicknessMaps", PlateThicknessMaps),
            new EnvironementMapDto("ContinentalIdMap", ContinentalIdMap),
        };

        foreach (var environementMapDto in maps)
        {
            if (!col.Update(environementMapDto))
            {
                col.Insert(environementMapDto);
            }
        }
    }

    private RenderTexture NewTexture(int channels, int layers)
    {
        var format = channels switch
        {
            1 => RenderTextureFormat.RFloat,
            2 => RenderTextureFormat.RGFloat,
            _ => RenderTextureFormat.ARGBFloat
        };
        return new RenderTexture(512, 512, 0, format, 0).ResetTexture(layers).Initialize();
    }

    private class EnvironementMapDto
    {
        public string Name { get; set; }
        public Color[][] Data { get; set; }

        public EnvironementMapDto(string name, RenderTexture rt)
        {
            Name = name;
            Data = rt.CachedTextures().Select(x => x.GetPixels()).ToArray();
        }

        public void LoadMap(RenderTexture rt)
        {
            rt.Initialize(Data);
        }
    }
}
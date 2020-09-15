using LiteDB;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class WorldDataStore : MonoBehaviour, IDailyProcess
{
    public string WorldName = "Hills";
    public float UpdateMilliseconds = 3;

    private string _connectionString;
    private bool _hasDayBeenProcessed = false;

    void Start()
    {
        BsonMapper.Global.MaxDepth = 10000;
        if (!Directory.Exists($"{Application.persistentDataPath}/DB"))
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/DB");
        }
        _connectionString = $"{Application.persistentDataPath}/DB/WorldSaves.db";
    }

    public bool HasDayBeenProccessed()
    {
        return _hasDayBeenProcessed;
    }

    public void ProcessDay()
    {
        _hasDayBeenProcessed = false;
        StartCoroutine(SaveWorld());
    }

    public void LoadWorld()
    {
        WorldSaveDto save;
        using (var db = new LiteDatabase($"{Application.persistentDataPath}/DB/WorldSaves.db"))
        {

            save = db.GetCollection<WorldSaveDto>("WorldSave").FindOne(x => x.WorldName == WorldName) ??
                new WorldSaveDto { Day = 0, WorldName = WorldName, Plants = new PlantDto[] { } };
        }

        Singleton.TimeService.Day = save.Day;
        foreach(var plant in save.Plants)
        {
            PlantFactory.Build(plant);
        }
    } 

    public IEnumerator SaveWorld()
    {
        var timer = new Stopwatch();
        timer.Restart();

        var plants = Singleton.PlantSearchService.GetAllPlants();
        var plantDtos = new List<PlantDto>();
        foreach (var plant in plants)
        {
            yield return new WaitForEndOfFrame();
            if (timer.ElapsedMilliseconds > UpdateMilliseconds)
            {
                timer.Restart();
            }
            plantDtos.Add(plant.ToDto());
        }

        var save = new WorldSaveDto
        {
            WorldName = WorldName,
            Day = Singleton.TimeService.Day,
            Plants = Singleton.PlantSearchService.GetAllPlants().Select(x => x.ToDto()).ToArray(),
        };

        var task = Task.Factory.StartNew(() => SaveWorld(save));

        timer.Stop();
        _hasDayBeenProcessed = true;
    }

    private void SaveWorld(WorldSaveDto newSave)
    {
        using (var db = new LiteDatabase(_connectionString))
        {
            var collection = db.GetCollection<WorldSaveDto>("WorldSave");
            collection.EnsureIndex(x => x.WorldName);
            collection.DeleteMany(x => x.WorldName == WorldName);
            collection.Insert(newSave);
        }
    }
}

public class WorldSaveDto
{
    public string WorldName { get; set; }
    public int Day { get; set; }
    public PlantDto[] Plants { get; set; }
}

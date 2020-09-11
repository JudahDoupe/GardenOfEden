using LiteDB;
using System.IO;
using System.Linq;
using UnityEngine;

public class WorldDataStore : MonoBehaviour, IDailyProcess
{
    void Start()
    {
        BsonMapper.Global.MaxDepth = 10000;
    }

    public bool HasDayBeenProccessed()
    {
        return true;
    }

    public void ProcessDay()
    {
        SaveWorld();
    }

    public void LoadWorld()
    {
        WorldSaveDto save;
        using (var db = new LiteDatabase($"{Application.persistentDataPath}/DB/WorldSaves.db"))
        {
            save = db.GetCollection<WorldSaveDto>("WorldSave").FindOne(Query.All(Query.Descending)) ??
                new WorldSaveDto { Day = 0, Plants = new PlantDto[] { } };
        }

        Singleton.TimeService.Day = save.Day;
        foreach(var plant in save.Plants)
        {
            PlantFactory.Build(plant);
        }
    } 

    public void SaveWorld()
    {
        if (!Directory.Exists($"{Application.persistentDataPath}/DB"))
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/DB");
        }

        using (var db = new LiteDatabase($"{Application.persistentDataPath}/DB/WorldSaves.db"))
        {
            var save = new WorldSaveDto
            {
                Day = Singleton.TimeService.Day,
                Plants = Singleton.PlantSearchService.GetAllPlants().Select(x => x.ToDto()).ToArray(),
            };
            var collection = db.GetCollection<WorldSaveDto>("WorldSave");
            collection.DeleteAll();
            collection.EnsureIndex(x => x.Day);
            collection.Insert(save);
        }
    }
}

public class WorldSaveDto
{
    public int Day { get; set; }
    public PlantDto[] Plants { get; set; }
}

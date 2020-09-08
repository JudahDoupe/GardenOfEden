using LiteDB;
using System.IO;
using UnityEngine;

public static class PlantDnaDataStore
{
    public static void SaveDna(PlantDnaDto dna)
    {
        if (!Directory.Exists($"{Application.persistentDataPath}/DB"))
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/DB");
        }

        using (var db = new LiteDatabase($"{Application.persistentDataPath}/DB/Dna.db"))
        {
            var collection = db.GetCollection<PlantDnaDto>("PlantDna");
            collection.EnsureIndex(x => x.Name);
            collection.Insert(dna);
        }
    }

    public static PlantDnaDto LoadDna(string plantName)
    {
        using (var db = new LiteDatabase($"{Application.persistentDataPath}/DB/Dna.db"))
        {
            return db.GetCollection<PlantDnaDto>("PlantDna").FindOne(x => x.Name == plantName);
        }
    }
}
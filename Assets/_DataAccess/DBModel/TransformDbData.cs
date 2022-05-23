using LiteDB;
using UnityEngine;

public class TransformDbData
{
    public BsonArray Position { get; set; }
    public BsonArray Rotation { get;  set; }
    public BsonArray Scale { get;  set; }
}

public static class TransformDbDataExtentions
{
    public static Vector3 Position(this TransformDbData dbData)
    {
        return new Vector3((float)dbData.Position.AsArray[0].AsDouble, (float)dbData.Position.AsArray[1].AsDouble, (float)dbData.Position.AsArray[2].AsDouble);
    }
    public static Quaternion Rotation(this TransformDbData dbData)
    {
        return new Quaternion((float)dbData.Rotation.AsArray[0].AsDouble, (float)dbData.Rotation.AsArray[1].AsDouble, (float)dbData.Rotation.AsArray[2].AsDouble, (float)dbData.Rotation.AsArray[3].AsDouble);
    }
    public static Vector3 Scale(this TransformDbData dbData)
    {
        return new Vector3((float)dbData.Scale.AsArray[0].AsDouble, (float)dbData.Scale.AsArray[1].AsDouble, (float)dbData.Scale.AsArray[2].AsDouble);
    }
}
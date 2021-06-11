using LiteDB;
using UnityEngine;

public class TransformDto
{
    public BsonArray Position { get; set; }
    public BsonArray Rotation { get;  set; }
    public BsonArray Scale { get;  set; }
}

public static class TransformDtoExtentions
{
    public static Vector3 Position(this TransformDto dto)
    {
        return new Vector3((float)dto.Position.AsArray[0].AsDouble, (float)dto.Position.AsArray[1].AsDouble, (float)dto.Position.AsArray[2].AsDouble);
    }
    public static Quaternion Rotation(this TransformDto dto)
    {
        return new Quaternion((float)dto.Rotation.AsArray[0].AsDouble, (float)dto.Rotation.AsArray[1].AsDouble, (float)dto.Rotation.AsArray[2].AsDouble, (float)dto.Rotation.AsArray[3].AsDouble);
    }
    public static Vector3 Scale(this TransformDto dto)
    {
        return new Vector3((float)dto.Scale.AsArray[0].AsDouble, (float)dto.Scale.AsArray[1].AsDouble, (float)dto.Scale.AsArray[2].AsDouble);
    }
}
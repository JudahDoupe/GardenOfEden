using UnityEngine;

public static class GrowthConditions
{
    public static bool IsLevel(this Node node)
    {
        var v = node.transform.rotation.eulerAngles;
        var flat = Quaternion.Euler(0, v.y, v.z);
        return Quaternion.Angle(node.transform.rotation, flat) < 0.00001f;
    }
    public static bool IsPlantOlder(this Node node, float age)
    {
        return node.Plant.Age > age;
    }
    public static bool IsPlantYounger(this Node node, float age)
    {
        return node.Plant.Age < age;
    }
    public static bool IsType(this Node node, string type)
    {
        return node.Type.ToLower() == type.ToLower();
    }
}
using System.Linq;

public enum EnvironmentMapType
{
    [EnvironmentMapMetaData(name: "WaterMap", channels: 4)]
    WaterMap,
    [EnvironmentMapMetaData(name: "WaterSourceMap", channels: 4)]
    WaterSourceMap,
    [EnvironmentMapMetaData(name: "LandHeightMap")]
    LandHeightMap,
    [EnvironmentMapMetaData(name: "PlateThicknessMaps")]
    PlateThicknessMaps,
    [EnvironmentMapMetaData(name: "TmpPlateThicknessMaps")]
    TmpPlateThicknessMaps,
    [EnvironmentMapMetaData(name: "ContinentalIdMap")]
    ContinentalIdMap,
    [EnvironmentMapMetaData(name: "TmpContinentalIdMap")]
    TmpContinentalIdMap,
}

public static class EnvironmentMapTypeExtensions
{
    public static EnvironmentMapMetaData MetaData(this EnvironmentMapType type)
    {
        var enumType = typeof(EnvironmentMapType);
        var memberInfos = enumType.GetMember(type.ToString());
        var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
        var attribute = (EnvironmentMapMetaDataAttribute)enumValueMemberInfo.GetCustomAttributes(typeof(EnvironmentMapMetaDataAttribute), false)[0];
        return attribute.MetaData;
    }
}
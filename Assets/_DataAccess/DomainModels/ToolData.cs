using System;
using Unity.Mathematics;

public class ToolData
{
    public ToolData(string name)
    {
        Name = name;
        UseState = new Signal<UseStateType>(UseStateType.Locked);
    }

    public ToolData(ToolsDbData dbData)
    {
        Name = dbData.Name;
        UseState = new Signal<UseStateType>((UseStateType)dbData.UseState);
    }

    public string Name { get; }
    public Signal<UseStateType> UseState { get; }

    public void Unlock() => UseState.Publish((UseStateType)math.max((int)UseState.Value, (int)UseStateType.Unlocked));
    public void Use() => UseState.Publish((UseStateType)math.max((int)UseState.Value, (int)UseStateType.Used));

    public ToolsDbData ToDbData()
        => new()
        {
            Name = Name,
            UseState = (int)UseState.Value,
        };
}

public enum UseStateType
{
    Locked = 0,
    Unlocked = 1,
    Used = 2
}


[Serializable]
public class ToolsDbData
{
    public string Name;
    public int UseState;
}
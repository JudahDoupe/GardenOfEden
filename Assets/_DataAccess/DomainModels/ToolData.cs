using System;
using Unity.Mathematics;

public class ToolData
{
    public string Name { get; }
    public UseState UseState { get; private set; }

    public void Unlock() => UseState = (UseState)math.max((int)UseState, (int)UseState.Unlocked);
    public void Use() => UseState = (UseState)math.max((int)UseState, (int)UseState.Used);

    public ToolData(string name)
    {
        Name = name;
        UseState = UseState.Locked;
    }
    
    public ToolData(ToolsDbData dbData)
    {
        Name = dbData.Name;
        UseState = (UseState)dbData.UseState;
    }
    
    public ToolsDbData ToDbData() =>
        new()
        {
            Name = Name,
            UseState = (int)UseState,
        };
}

public enum UseState
{
    Locked = 0,
    Unlocked = 2,
    Used = 1,
}


[Serializable]
public class ToolsDbData
{
    public string Name;
    public int UseState;
}

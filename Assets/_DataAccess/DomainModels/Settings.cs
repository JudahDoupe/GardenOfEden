using System;

public class Settings
{
    public Settings(SettingsDbData dbData)
    {
        ScrollSpeed = dbData.ScrollSpeed;
        DragSpeed = dbData.DragSpeed;
    }

    public float ScrollSpeed { get; set; }
    public float DragSpeed { get; set; }

    public SettingsDbData ToDbData()
        => new()
        {
            ScrollSpeed = ScrollSpeed,
            DragSpeed = DragSpeed
        };
}

[Serializable]
public class SettingsDbData
{
    public float ScrollSpeed = 5f;
    public float DragSpeed = 5f;
}
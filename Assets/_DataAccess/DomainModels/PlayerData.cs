using System.Collections.Generic;
using System.Linq;

public class PlayerData
{
    public string CurrentPlanetName { get; set; }
    public Settings Settings { get; set; }
    public List<string> PlanetNames { get; }

    public PlayerData(PlayerDbData dbData)
    {
        CurrentPlanetName = dbData.CurrentPlanetName;
        PlanetNames = dbData.PlanetNames.ToList();
        Settings = new Settings(dbData.Settings);
    }

    public PlayerDbData ToDbData() =>
        new()
        {
            CurrentPlanetName = CurrentPlanetName,
            PlanetNames = PlanetNames.ToArray(),
            Settings = Settings.ToDbData(),
        };
}

public class Settings
{
    public float ScrollSpeed { get; set; }
    public float DragSpeed { get; set; }
    
    public Settings(SettingsDbData dbData)
    {
        ScrollSpeed = dbData.ScrollSpeed;
        DragSpeed = dbData.DragSpeed;
    }
    
    public SettingsDbData ToDbData() =>
        new()
        {
            ScrollSpeed = ScrollSpeed,
            DragSpeed = DragSpeed,
        };
}
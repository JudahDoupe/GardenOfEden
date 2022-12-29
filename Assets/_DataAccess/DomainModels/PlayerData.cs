using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerData
{
    public PlayerData(PlayerDbData dbData)
    {
        CurrentPlanetName = dbData.CurrentPlanetName;
        PlanetNames = dbData.PlanetNames.ToList();
        Settings = new Settings(dbData.Settings);
    }

    public string CurrentPlanetName { get; set; }
    public Settings Settings { get; }
    public List<string> PlanetNames { get; }

    public PlayerDbData ToDbData()
        => new()
        {
            CurrentPlanetName = CurrentPlanetName,
            PlanetNames = PlanetNames.ToArray(),
            Settings = Settings.ToDbData(),
        };
}

[Serializable]
public class PlayerDbData
{
    public string CurrentPlanetName = null;
    public string[] PlanetNames = Array.Empty<string>();
    public SettingsDbData Settings = new();
}
using System.Collections.Generic;
using System.Linq;

public class PlayerData
{
    public PlayerData(PlayerDbData dbData)
    {
        CurrentPlanetName = dbData.CurrentPlanetName;
        PlanetNames = dbData.PlanetNames.ToList();
    }

    public string CurrentPlanetName { get; set; }
    public List<string> PlanetNames { get; }

    public PlayerDbData ToDbData() =>
        new PlayerDbData
        {
            CurrentPlanetName = CurrentPlanetName,
            PlanetNames = PlanetNames.ToArray(),
        };
}
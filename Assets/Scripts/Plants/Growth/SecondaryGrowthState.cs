public class SecondaryGrowthState : IGrowthState
{
    public void Grow(Plant plant)
    {
        var growthInDays = EnvironmentApi.GetDate() - plant.LastUpdatedDate;
        var requiredSugar = Volume.FromCubicMeters(growthInDays / 10);
        if (plant.StoredSugar > requiredSugar)
        {
            plant.Trunk.Grow(growthInDays);
            plant.StoredSugar -= requiredSugar;
        }

        if (plant.StoredSugar > plant.SustainingSugar + Volume.FromCubicMeters(3))
        {
            plant.GrowthState = new ReproductionState();
        }
    }
}

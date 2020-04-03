public class SecondaryGrowthState : IGrowthState
{
    public void Grow(Plant plant)
    {
        var growthInDays = EnvironmentApi.GetDate() - plant.LastUpdatedDate;
        var requiredSugar = Volume.FromCubicMeters(growthInDays / 10);
        if (plant.StoredStarch > requiredSugar)
        {
            //plant.Trunk.Grow(growthInDays);
            plant.Root.Grow(growthInDays);
            plant.StoredStarch -= requiredSugar;
        }

        if (plant.StoredStarch > plant.SustainingSugar + Volume.FromCubicMeters(3))
        {
            plant.GrowthState = new ReproductionState();
        }
    }
}

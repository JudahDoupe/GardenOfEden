using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Root : Structure
{
    public void Start()
    {
        DI.RootService.AddRoots(this, AbsorbWater);
    }
    public void AbsorbWater(Volume water)
    {
        Plant.HasWaterBeenAbsorbed = true;
        Plant.StoredWater += water;
        Plant.TryPhotosynthesis();
    }
}

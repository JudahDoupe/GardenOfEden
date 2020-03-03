using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeRoot : Root
{
    private void Start()
    {
        DI.RootService.AddRoots(Plant, AbsorbWater);
    }

    private void AbsorbWater(Volume water)
    {
         Plant.HasWaterBeenAbsorbed = true;
         Plant.StoredWater += water;
        Plant.TryPhotosynthesis();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : TimeTracker
{
    void Start()
    {
        
    }

    public Volume Grow(Volume availableSugar)
    {
        return availableSugar;
    }
}

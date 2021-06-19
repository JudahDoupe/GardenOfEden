using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var coord = new Coordinate(transform.parent.position, Planet.LocalToWorld);
        coord.Altitude = 1000;
        transform.position = coord.Global(Planet.LocalToWorld);
    }
}

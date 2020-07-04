using System;
using System.Linq;
using UnityEngine;

public class PlantApi : MonoBehaviour
{
    /* API */

    public static int GetSpeciesPopulation(int speciesId)
    {
        return FindObjectsOfType<Plant>().Count(p => p.PlantDna.SpeciesId == speciesId);
    }
}

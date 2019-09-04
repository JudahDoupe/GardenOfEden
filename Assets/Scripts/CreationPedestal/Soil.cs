using System;
using UnityEngine;

public class Soil : MonoBehaviour
{
    public void Clicked(Vector3 hitPosition)
    {
        var pedestal = transform.parent.GetComponent<PlantCreationPedestal>();
        if (pedestal.SelectedDna != null && pedestal.Plant == null)
        {
            var dna = new PlantDNA
            {
                Trunk = pedestal.SelectedDna.Dna,
                GestationPeriod = 2,
                MaxOffspring =  10,
                Name = "Dingus",
                SpeciesId = new Guid(),
            };

            pedestal.Plant = new GameObject().AddComponent<Plant>().GetComponent<Plant>();
            pedestal.Plant.transform.position = hitPosition;
            pedestal.Plant.DNA = dna;
            pedestal.Plant.IsAlive = false;

            pedestal.Plant.Trunk = Structure.Create(pedestal.Plant, dna.Trunk);
            pedestal.Plant.Trunk.transform.parent = pedestal.Plant.transform;
            pedestal.Plant.Trunk.transform.localPosition = Vector3.zero;
            pedestal.Plant.Trunk.transform.localEulerAngles = new Vector3(-90,0,0);

            pedestal.Plant.transform.parent = transform;
            pedestal.Plant.Trunk.gameObject.AddComponent<StructureSelector>();
        }
    }
}

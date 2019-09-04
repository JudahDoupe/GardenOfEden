using UnityEngine;

public class DoneButton : MonoBehaviour
{
    public void Clicked()
    {
        var pedestal = transform.parent.GetComponent<PlantCreationPedestal>();

        PlantService.SpawnSpecies(pedestal.Plant.GetDNA());

        pedestal.EndCreation();
    }
}

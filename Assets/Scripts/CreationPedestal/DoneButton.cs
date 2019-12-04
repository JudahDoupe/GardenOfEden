using UnityEngine;

public class DoneButton : MonoBehaviour
{
    public void Clicked()
    {
        var pedestal = transform.parent.GetComponent<PlantCreationPedestal>();

        PlantApi.SpawnSpecies(pedestal.Plant.GenerateDNA());

        pedestal.EndCreation();
    }
}

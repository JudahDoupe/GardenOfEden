using UnityEngine;

public class DoneButton : MonoBehaviour
{
    public void Clicked()
    {
        var pedestal = transform.parent.GetComponent<PlantCreationPedestal>();
        var dna = pedestal.Plant.GenerateDNA();

        for (int i = 0; i < 5; i++)
        {
            var randomLocation = Random.insideUnitSphere * 25;
            var worldPosition = transform.position + randomLocation;

            PlantApi.DropSeed(dna, worldPosition);
        }

        pedestal.EndCreation();
    }
}

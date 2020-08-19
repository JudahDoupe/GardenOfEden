public class LightAbsorberUpdateVisitor : IPlantVisitor
{
    public void VisitPlant(Plant plant)
    {
        UpdateMeshRecursively(plant);
    }

    private void UpdateMeshRecursively(Node node)
    {
        foreach (var branchNode in node.Branches)
        {
            UpdateMeshRecursively(branchNode);
        }
        
        updateNode(node);
    }
    private void updateNode(Node node)
    {
        Singleton.LightService.UpdateAbsorber(node);
    }
}
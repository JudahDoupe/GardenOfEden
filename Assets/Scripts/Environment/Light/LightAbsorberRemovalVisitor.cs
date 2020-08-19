public class LightAbsorberRemovalVisitor : IPlantVisitor
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
        
        removeNode(node);
    }
    private void removeNode(Node node)
    {
        Singleton.LightService.RemoveAbsorber(node);
    }
}
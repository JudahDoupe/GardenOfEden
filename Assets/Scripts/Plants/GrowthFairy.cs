using UnityEngine;

public class GrowthFairy : IVisitor
{
    public void VisitPlant(Plant plant)
    {

    }


    private void GrowShoot(Node node)
    {
    }

    private void GrowBud(Bud bud)
    {

    }

    private void GrowStem(Bud bud)
    {
    }

    private void GrowLeaf(Stem stem)
    {
    }

}

public interface IVisitor
{
    void VisitPlant(Plant plant); 
}
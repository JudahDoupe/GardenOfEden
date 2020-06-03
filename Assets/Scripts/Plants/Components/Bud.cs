public class Bud : Node
{
    public static Bud Create(Node baseNode)
    {
        return Node.Create<Bud>(baseNode);
    }
}

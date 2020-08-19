
public static class PlantMessageBus
{
    public static MessageBus<Plant> NewPlant = new MessageBus<Plant>();
    public static MessageBus<Plant> PlantDeath = new MessageBus<Plant>();

    public static MessageBus<Node> NewNode = new MessageBus<Node>();
    public static MessageBus<Node> NodeDeath = new MessageBus<Node>();
    public static MessageBus<Node> NodeUpdate = new MessageBus<Node>();
}
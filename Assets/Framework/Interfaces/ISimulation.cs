public interface ISimulation
{
    bool IsActive { get; }
    void Enable();
    void Disable();
}

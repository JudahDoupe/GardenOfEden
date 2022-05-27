public interface ITool
{
    bool IsActive { get; }
    void Enable();
    void Disable();
}

public interface ITool
{
    bool IsActive { get; }
    void Unlock();
    void Enable();
    void Disable();
}

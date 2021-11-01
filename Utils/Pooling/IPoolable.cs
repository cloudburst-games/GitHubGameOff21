// IPoolable interface: contract specifying what an object must implement for it to be poolable.


public interface IPoolable
{
    void Init();
    void Hide();
    void Show();
    void QueueFree();
    void Term();
}
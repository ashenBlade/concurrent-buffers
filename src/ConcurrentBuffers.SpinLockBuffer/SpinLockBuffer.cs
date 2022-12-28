using ConcurrentBuffers.Core;

namespace ConcurrentBuffers.SpinLockBuffer;

/// <summary>
/// IConcurrentBuffer implementation uses SpinLock
/// in order to avoid concurrency troubles
/// </summary>
public class SpinLockBuffer<T>: IConcurrentBuffer<T>
{
    private List<T> Buffer { get; } = new();
    
    private SpinLock _lock;
    
    public void Add(T item)
    {
        var lockTaken = false;
        try
        {
            _lock.Enter(ref lockTaken);
            Buffer.Add(item);
        }
        finally
        {
            if (lockTaken)
            {
                _lock.Exit();
            }
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        var lockTaken = false;
        try
        {
            _lock.Enter(ref lockTaken);
            Buffer.AddRange(items);
        }
        finally
        {
            if (lockTaken)
            {
                _lock.Exit();
            }
        }
    }

    public IEnumerable<T> Flush()
    {
        T[] stored;
        var lockTaken = false;
        try
        {
            _lock.Enter(ref lockTaken);
            stored = Buffer.ToArray();
            Buffer.Clear();
        }
        finally
        {
            if (lockTaken)
            {
                _lock.Exit();
            }
        }
        return stored;
    }
}
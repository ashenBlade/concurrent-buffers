using ConcurrentBuffers.Abstractions;

namespace ConcurrentBuffers.Buffers;

/// <summary>
/// IConcurrentBuffer implementation uses SpinLock
/// in order to avoid concurrency troubles
/// </summary>
public class SpinLockBuffer<T>: IConcurrentBuffer<T>
{
    private readonly List<T> _buffer = new();
    private SpinLock _lock;
    
    public void Add(T item)
    {
        var lockTaken = false;
        try
        {
            _lock.Enter(ref lockTaken);
            _buffer.Add(item);
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
            _buffer.AddRange(items);
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
            stored = _buffer.ToArray();
            _buffer.Clear();
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
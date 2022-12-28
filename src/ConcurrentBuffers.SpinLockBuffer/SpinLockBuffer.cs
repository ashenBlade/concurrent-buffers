using ConcurrentBuffers.Core;

namespace ConcurrentBuffers.SpinLockBuffer;

/// <summary>
/// IConcurrentBuffer implementation uses SpinLock
/// in order to avoid concurrency troubles
/// </summary>
public class SpinLockBuffer<T>: IConcurrentBuffer<T>
{
    private List<T> Buffer { get; } = new();
    private SpinLock Lock { get; }
    
    public void Add(T item)
    {
        var lockTaken = false;
        Lock.Enter(ref lockTaken);
        try
        {
            Buffer.Add(item);
        }
        finally
        {
            if (lockTaken)
            {
                Lock.Exit();
            }
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        var lockTaken = false;
        Lock.Enter(ref lockTaken);
        try
        {
            Buffer.AddRange(items);
        }
        finally
        {
            if (lockTaken)
            {
                Lock.Exit();
            }
        }
    }

    public IEnumerable<T> Flush()
    {
        T[] stored;
        var lockTaken = false;
        Lock.Enter(ref lockTaken);
        try
        {
            stored = Buffer.ToArray();
            Buffer.Clear();
        }
        finally
        {
            if (lockTaken)
            {
                Lock.Exit();
            }
        }
        return stored;
    }
}
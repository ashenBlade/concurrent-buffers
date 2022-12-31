using ConcurrentBuffers.Abstractions;

namespace ConcurrentBuffers;

public class LockBuffer<T>: IConcurrentBuffer<T>
{
    private readonly List<T> _buffer = new(); 
    public void Add(T item)
    {
        lock (_buffer)
        {
            _buffer.Add(item);
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        lock (_buffer)
        {
            _buffer.AddRange(items);
        }
    }

    public IEnumerable<T> Flush()
    {
        T[] elements;
        lock (_buffer)
        {
            elements = _buffer.ToArray();
            _buffer.Clear();
        }

        return elements;
    }
}
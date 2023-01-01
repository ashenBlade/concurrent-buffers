using System.Collections.Concurrent;
using ConcurrentBuffers.Abstractions;

namespace ConcurrentBuffers;

/// <summary>
/// Concurrent buffer, that uses ConcurrentBag to store items
/// </summary>
/// <inheritdoc cref="IConcurrentBuffer{T}"/>
public class ConcurrentBagBuffer<T>: IConcurrentBuffer<T>
{
    private volatile ConcurrentBag<T> _bag = new();
    public void Add(T item)
    {
        _bag.Add(item);
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            _bag.Add(item);
        }
    }

    public IEnumerable<T> Flush()
    {
        var bag = new ConcurrentBag<T>();
        ConcurrentBag<T> old;
        do
        {
            old = _bag;
        } while (Interlocked.CompareExchange(ref _bag, bag, old) != old);

        return old;
    }
}
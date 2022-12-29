namespace ConcurrentBuffers.Abstractions;

/// <summary>
/// Interface, that represents buffer with ability to insert and flush
/// </summary>
/// <typeparam name="T">Item type stored in buffer</typeparam>
public interface IBuffer<T>
{
    /// <summary>
    /// Add item to buffer
    /// </summary>
    /// <param name="item">Item to add</param>
    void Add(T item);

    /// <summary>
    /// Add multiple items to buffer
    /// </summary>
    /// <param name="items">Items to add</param>
    void AddRange(IEnumerable<T> items);

    /// <summary>
    /// Get all items in buffer
    /// </summary>
    /// <returns>Items stored in buffer</returns>
    /// <remarks>After call, stored items are deleted from queue</remarks>
    IEnumerable<T> Flush();
}
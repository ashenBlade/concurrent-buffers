namespace ConcurrentBuffers.Abstractions;

/// <summary>
/// Interface, represents concurrent buffer
/// </summary>
/// <typeparam name="T">Item type, buffer stores</typeparam>
public interface IConcurrentBuffer<T>: IBuffer<T>
{ }
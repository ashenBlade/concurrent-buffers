namespace ConcurrentBuffers.Abstractions;

/// <summary>
/// Interface, represents concurrent buffer
/// </summary>
/// <typeparam name="T">Item type, buffer storesRE</typeparam>
public interface IConcurrentBuffer<T>: IBuffer<T>
{ }
namespace ConcurrentBuffers.Abstractions;

/// <summary>
/// Interface, that represents thread-safe buffer
/// </summary>
/// <inheritdoc cref="IBuffer{T}"/>
public interface IConcurrentBuffer<T>: IBuffer<T>
{ }
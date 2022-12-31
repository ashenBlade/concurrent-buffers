namespace ConcurrentBuffers.Tests;

public class ConcurrentBagBufferTests: ConcurrentBufferTestsBase<ConcurrentBagBuffer<int>>
{
    public override ConcurrentBagBuffer<int> Buffer { get; } = new();
}
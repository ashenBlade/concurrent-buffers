namespace ConcurrentBuffers.Tests;

public class LockBufferTests: ConcurrentBufferTestsBase<LockBuffer<int>>
{
    public override LockBuffer<int> Buffer { get; } = new();
}
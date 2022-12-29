namespace ConcurrentBuffers.Buffers.Tests;

public class SpinLockBufferTests: ConcurrentBufferTestsBase<SpinLockBuffer<int>>
{
    public override SpinLockBuffer<int> Buffer { get; } = new();
}
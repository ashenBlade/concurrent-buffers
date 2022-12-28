using Xunit;

namespace ConcurrentBuffers.SpinLockBuffer.Tests;

public class UnitTest1
{
    private SpinLockBuffer<int> _buffer = new();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(10)]
    public void FlushAfterAdd_WithSingleElement_ShouldReturnAddedElement(int expected)
    {
        _buffer.Add(expected);
        var flushed = _buffer
                     .Flush()
                     .ToArray();
        
        Assert.Single(flushed);
        var actual = flushed.First();
        Assert.Equal(expected, actual);
    }
}
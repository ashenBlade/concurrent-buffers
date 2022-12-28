using Xunit;
using Xunit.Sdk;

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

    public static IEnumerable<int> GenerateRandomInts(int amount) => 
        Enumerable
           .Repeat(0, amount)
           .Select(_ => Random.Shared.Next());

    public static IEnumerable<object[]> RandomInts =>
        new[]
        {
            new[] {GenerateRandomInts(1)}, 
            new[] {GenerateRandomInts(2)}, 
            new[] {GenerateRandomInts(3)}, 
            new[] {GenerateRandomInts(4)}, 
            new[] {GenerateRandomInts(5)}, 
            new[] {GenerateRandomInts(10)}, 
            new[] {GenerateRandomInts(20)}, 
            new[] {GenerateRandomInts(30)}, 
            new[] {GenerateRandomInts(50)}, 
        };

    [Theory]
    [MemberData(nameof(RandomInts))]
    public void FlushAfterAddRange_WithMultipleElements_ShouldReturnAllAddedElements(IEnumerable<int> values)
    {
        var expected = values.ToArray();
        _buffer.AddRange(expected);
        var actual = _buffer.Flush();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(RandomInts))]
    public void FlushAfterFlush_ShouldReturnNoElements(IEnumerable<int> values)
    {
        _buffer.AddRange(values);
        _buffer.Flush();
        var actual = _buffer.Flush();
        Assert.Empty(actual);
    }

    [Theory]
    [MemberData(nameof(RandomInts))]
    public async Task Add_WithConcurrentCalls_ShouldAddAllElements(IEnumerable<int> values)
    {
        var expected = values.ToHashSet();
        await Task.WhenAll(expected
                          .Select(v => Task.Run(() => _buffer.Add(v)))
                          .ToArray());
        var actual = _buffer.Flush()
                            .ToHashSet();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(RandomInts))]
    public async Task AddRange_WithConcurrentCalls_ShouldAddAllElements(IEnumerable<int> values)
    {
        var expected = values.ToHashSet();
        await Task.WhenAll(expected.Chunk(5)
                                   .Select(r => Task.Run(() => _buffer.AddRange(r))));
        var actual = _buffer.Flush()
                            .ToHashSet();
        Assert.Equal(expected, actual);
    }
}
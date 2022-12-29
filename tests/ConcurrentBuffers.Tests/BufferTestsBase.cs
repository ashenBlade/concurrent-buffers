using ConcurrentBuffers.Abstractions;
using Xunit;

namespace ConcurrentBuffers.Tests;

public abstract class BufferTestsBase<TBuffer> where TBuffer: IBuffer<int>
{
    public abstract TBuffer Buffer { get; }
    
    [Theory(DisplayName = "Fulsh")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(10)]
    public void FlushAfterAdd_WithSingleElement_ShouldReturnAddedElement(int expected)
    {
        Buffer.Add(expected);
        var flushed = Buffer
                     .Flush()
                     .ToArray();
        
        Assert.Single(flushed);
        var actual = flushed.First();
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<int> GenerateRandomNumbers(int amount) => 
        Enumerable
           .Repeat(0, amount)
           .Select(_ => Random.Shared.Next());

    public static IEnumerable<object[]> RandomNumbers =>
        new[]
        {
            new[] {GenerateRandomNumbers(1)}, 
            new[] {GenerateRandomNumbers(2)}, 
            new[] {GenerateRandomNumbers(3)}, 
            new[] {GenerateRandomNumbers(4)}, 
            new[] {GenerateRandomNumbers(5)}, 
            new[] {GenerateRandomNumbers(10)}, 
            new[] {GenerateRandomNumbers(20)}, 
            new[] {GenerateRandomNumbers(30)}, 
            new[] {GenerateRandomNumbers(50)}, 
        };

    [Theory]
    [MemberData(nameof(RandomNumbers))]
    public void FlushAfterAddRange_WithMultipleElements_ShouldReturnAllAddedElements(IEnumerable<int> values)
    {
        var expected = values.ToArray();
        Buffer.AddRange(expected);
        var actual = Buffer.Flush();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(RandomNumbers))]
    public void FlushAfterFlush_ShouldReturnNoElements(IEnumerable<int> values)
    {
        Buffer.AddRange(values);
        Buffer.Flush();
        var actual = Buffer.Flush();
        Assert.Empty(actual);
    }
}
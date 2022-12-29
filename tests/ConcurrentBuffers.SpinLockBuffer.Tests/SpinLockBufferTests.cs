using System.Collections.Concurrent;
using Xunit;
using Xunit.Abstractions;

namespace ConcurrentBuffers.SpinLockBuffer.Tests;

public class SpinLockBufferTests
{
    private readonly SpinLockBuffer<int> _buffer = new();
    
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
        _buffer.AddRange(expected);
        var actual = _buffer.Flush();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(RandomNumbers))]
    public void FlushAfterFlush_ShouldReturnNoElements(IEnumerable<int> values)
    {
        _buffer.AddRange(values);
        _buffer.Flush();
        var actual = _buffer.Flush();
        Assert.Empty(actual);
    }

    [Theory]
    [MemberData(nameof(RandomNumbers))]
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
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(300)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(2000)]
    [InlineData(5000)]
    public async Task AddRange_WithConcurrentCalls_ShouldAddAllElements(int numbersCount)
    {
        var expected = GenerateRandomNumbers(numbersCount).ToHashSet();
        await Task.WhenAll(expected.Chunk(5)
                                   .Select(r => Task.Run(() => _buffer.AddRange(r))));
        var actual = _buffer.Flush()
                            .ToHashSet();
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<int> NumbersCount => 
        new[] {100, 150, 200, 250, 300, 500, 1000, 1500, 3000, 5000, 10000};

    public static IEnumerable<int> ParallelCallsCount =>
        new[] { 2, 3, 4, 5, 10, 15, 20, 25, 30, 50, 100, 120, 150, 200, 300 };

    public static IEnumerable<object[]> NumbersCountParallelCallsCount =>
        NumbersCount
           .SelectMany(nc => ParallelCallsCount
                          .Select(pcc => new object[] {nc, pcc}));

    const int MaxWaitTime = 1000;
    
    // Timeout for deadlock check
    [Theory(Timeout = MaxWaitTime * 10)]
    [MemberData(nameof(NumbersCountParallelCallsCount))]
    public void Flush_WithConcurrentAddCalls_ShouldFlushAllAddedElements(int numbersCount, int concurrentFlushCalls)
    {
        var expected = GenerateRandomNumbers(numbersCount).ToHashSet();
        var flushed = new ConcurrentQueue<IEnumerable<int>>();
        Task.WaitAll(expected
                    .Chunk(10)
                    .Select((chunk, i) => Task.Run(async () =>
                     {
                         await Task.Delay(Random.Shared.Next(0, MaxWaitTime));
                         if (Random.Shared.Next(0, 1) == 0)
                         {
                             foreach (var number in chunk)
                             {
                                 _buffer.Add(number);
                             }
                         }
                         else
                         {
                             _buffer.AddRange(chunk);
                         }
                     }))
                    .Concat(Enumerable.Range(0, concurrentFlushCalls)
                                      .Select(i => Task.Run(async () =>
                                       {
                                           await Task.Delay(Random.Shared.Next(0, MaxWaitTime));
                                           flushed.Enqueue(_buffer.Flush());
                                       })))
                    .ToArray());
        flushed.Enqueue(_buffer.Flush());
        var actual = flushed.ToArray()
                            .SelectMany(i => i)
                            .ToHashSet();
        Assert.Equal(expected, actual);
    }
}
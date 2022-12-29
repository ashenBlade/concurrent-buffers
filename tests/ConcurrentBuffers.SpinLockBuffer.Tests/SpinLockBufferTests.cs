using System.Collections.Concurrent;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ConcurrentBuffers.SpinLockBuffer.Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;
    private SpinLockBuffer<int> _buffer = new();

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

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

    public static IEnumerable<object[]> ManyRandomInts =>
        new[]
        {
            new object[] {GenerateRandomInts(100)}, new object[] {GenerateRandomInts(200)},
            new object[] {GenerateRandomInts(300)}, new object[] {GenerateRandomInts(500)},
            new object[] {GenerateRandomInts(1000)}, new object[] {GenerateRandomInts(2000)},
            new object[] {GenerateRandomInts(5000)},
        };

    [Theory]
    [MemberData(nameof(ManyRandomInts))]
    public async Task AddRange_WithConcurrentCalls_ShouldAddAllElements(IEnumerable<int> values)
    {
        var expected = values.ToHashSet();
        await Task.WhenAll(expected.Chunk(5)
                                   .Select(r => Task.Run(() => _buffer.AddRange(r))));
        var actual = _buffer.Flush()
                            .ToHashSet();
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> ManyRandomValuesAndConcurrentFlushCallsCount =>
        new[]
        {
            new object[] {GenerateRandomInts(100), 2},
            new object[] {GenerateRandomInts(200), 10},
            new object[] {GenerateRandomInts(300), 300}, 
            new object[] {GenerateRandomInts(500), 60},
            new object[] {GenerateRandomInts(1000), 20},
            new object[] {GenerateRandomInts(2000), 11},
            new object[] {GenerateRandomInts(5000), 56},
            new object[] {GenerateRandomInts(1000), 765},
            new object[] {GenerateRandomInts(1000), 465},
        };

    public static IEnumerable<int> NumbersCount => 
        new[] {100, 150, 200, 250, 300, 500, 1000, 1500, 3000, 5000, 10000};

    public static IEnumerable<int> ParallelCallsCount =>
        new[] { 2, 3, 4, 5, 10, 15, 20, 25, 30, 50, 100, 120, 150, 200, 300 };

    public static IEnumerable<object[]> NumbersCountParallelCallsCount =>
        NumbersCount
           .SelectMany(nc => ParallelCallsCount
                          .Select(pcc => new object[] {nc, pcc}));

    [Theory]
    [MemberData(nameof(NumbersCountParallelCallsCount))]
    public void Flush_WithConcurrentAddCalls_ShouldFlushAllAddedElements(int numbersCount, int concurrentFlushCalls)
    {
        var expected = GenerateRandomInts(numbersCount).ToHashSet();
        var flushed = new ConcurrentQueue<IEnumerable<int>>();
        const int maxWaitTime = 1000;
        Task.WaitAll(expected
                    .Chunk(10)
                    .Select((chunk, i) => Task.Run(async () =>
                     {
                         await Task.Delay(Random.Shared.Next(0, maxWaitTime));
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
                                           await Task.Delay(Random.Shared.Next(0, maxWaitTime));
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
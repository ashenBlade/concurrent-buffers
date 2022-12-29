using System.Collections.Concurrent;
using ConcurrentBuffers.Abstractions;
using Xunit;

namespace ConcurrentBuffers.Buffers.Tests;

public abstract class ConcurrentBufferTestsBase<TConcurrentBuffer>
    : BufferTestsBase<TConcurrentBuffer> 
    where TConcurrentBuffer: IConcurrentBuffer<int>
{
    [Theory]
    [MemberData(nameof(RandomNumbers))]
    public async Task Add_WithConcurrentCalls_ShouldAddAllElements(IEnumerable<int> values)
    {
        var expected = values.ToHashSet();
        await Task.WhenAll(expected
                          .Select(v => Task.Run(() => Buffer.Add(v)))
                          .ToArray());
        var actual = Buffer.Flush()
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
                                   .Select(r => Task.Run(() => Buffer.AddRange(r))));
        var actual = Buffer.Flush()
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
                                 Buffer.Add(number);
                             }
                         }
                         else
                         {
                             Buffer.AddRange(chunk);
                         }
                     }))
                    .Concat(Enumerable.Range(0, concurrentFlushCalls)
                                      .Select(i => Task.Run(async () =>
                                       {
                                           await Task.Delay(Random.Shared.Next(0, MaxWaitTime));
                                           flushed.Enqueue(Buffer.Flush());
                                       })))
                    .ToArray());
        flushed.Enqueue(Buffer.Flush());
        var actual = flushed.ToArray()
                            .SelectMany(i => i)
                            .ToHashSet();
        Assert.Equal(expected, actual);
    }
}
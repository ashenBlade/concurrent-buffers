# ConcurrentBuffers

## What is ConcurrentBuffers

ConcurrentBuffers is a simple library that provides types, that represents buffers.
Buffer is a type with 3 common operations: `Add`, `AddRange` and `Flush`

- `Add`: add single item to buffer
- `AddRange`: add multiple items to buffer
- `Flush`: flush all stored items from buffer

## Example usage

```csharp
using ConcurrentBuffers;

var buffer = new SpinLockBuffer<int>();
buffer.Add(0);
buffer.AddRange(Enumerable.Range(1, 100));
var flushed = buffer.Flush();
foreach (var number in flushed)
{
    Console.WriteLine(number);
}
```

## Install

### dotnet cli

```shell
dotnet add dotnet add package ConcurrentBuffers --version 1.0.1
```

### Package Manager

```powershell
Install-Package ConcurrentBuffers -Version 1.0.1
```

## Contributing

Pull requests are welcome. 

If you create new implementations for `IBuffer<T>` or `IConcurrentBuffer<T>`, 
then provide unittests for it:
- Add new test class in tests/ConcurrentBuffers.Tests
- Inherit it from `ConcurrentBufferTestsBase` and implement abstract members
- If you have specific for your buffer tests, then write your own

## Implementations

- `SpinLockBuffer` - uses SpinLock to avoid race conditions
- `LockBuffer` - uses lock() construction
- `ConcurrentBagBuffer` - uses ConcurrentBag as items storage

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using LionFire.OpenCode.Serve.Benchmarks;

// Run all benchmarks in release mode
// Usage: dotnet run -c Release -- --filter *
// For quick testing: dotnet run -c Release -- --filter * --job short

#if DEBUG
Console.WriteLine("WARNING: Running benchmarks in DEBUG mode. Results will not be accurate.");
Console.WriteLine("Use 'dotnet run -c Release' for accurate benchmark results.");
Console.WriteLine();
#endif

var config = DefaultConfig.Instance;

// Parse command line for specific benchmark selection
if (args.Length == 0)
{
    // Show menu if no args
    Console.WriteLine("OpenCode SDK Performance Benchmarks");
    Console.WriteLine("=====================================");
    Console.WriteLine();
    Console.WriteLine("Available benchmarks:");
    Console.WriteLine("  1. JsonSerializationBenchmarks - DTO serialization/deserialization");
    Console.WriteLine("  2. MessageListBenchmarks - Message list operations");
    Console.WriteLine("  3. StreamingEventBenchmarks - SSE event parsing");
    Console.WriteLine("  4. All - Run all benchmarks");
    Console.WriteLine();
    Console.WriteLine("Run with: dotnet run -c Release -- --filter *BenchmarkName*");
    Console.WriteLine();

    // Run all by default
    BenchmarkSwitcher
        .FromAssembly(typeof(Program).Assembly)
        .Run(args, config);
}
else
{
    BenchmarkSwitcher
        .FromAssembly(typeof(Program).Assembly)
        .Run(args, config);
}

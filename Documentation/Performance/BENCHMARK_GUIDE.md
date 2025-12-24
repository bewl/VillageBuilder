# Benchmark Guide

## Overview

This guide explains how to create, run, and maintain benchmarks in the VillageBuilder project using BenchmarkDotNet.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Benchmark Structure](#benchmark-structure)
3. [Running Benchmarks](#running-benchmarks)
4. [Best Practices](#best-practices)
5. [Interpreting Results](#interpreting-results)
6. [Troubleshooting](#troubleshooting)

---

## Quick Start

### Running Existing Benchmarks

```bash
cd BenchmarkSuite1
dotnet run -c Release
```

**Important:** Always run benchmarks in **Release** mode, not Debug.

### Existing Benchmarks

1. **GameEngineSimulationBenchmark** - Measures `SimulateTick()` performance
2. **TextRenderingBenchmark** - Measures sidebar UI rendering

---

## Benchmark Structure

### Project Setup

Location: `BenchmarkSuite1/BenchmarkSuite1.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <OutputType>Exe</OutputType>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="BenchmarkDotNet" Version="0.15.2" />
    <PackageReference Include="Microsoft.VisualStudio.DiagnosticsHub.BenchmarkDotNetDiagnosers" Version="18.3.36714.1" />
    <PackageReference Include="Raylib-cs" Version="7.0.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\VillageBuilder.Engine\VillageBuilder.Engine.csproj" />
    <ProjectReference Include="..\VillageBuilder.Game\VillageBuilder.Game.csproj" />
  </ItemGroup>
</Project>
```

### Program.cs

```csharp
using BenchmarkDotNet.Running;
using VillageBuilder.Benchmarks;

namespace BenchmarkSuite1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Run all benchmarks in assembly using BenchmarkSwitcher for selective execution
            var _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
```

### Basic Benchmark Template

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.VSDiagnostics;

namespace VillageBuilder.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net90)]
    [CPUUsageDiagnoser]
    public class MyBenchmark
    {
        private MyClass _instance = null!;

        [GlobalSetup]
        public void Setup()
        {
            // One-time initialization (runs once before all benchmarks)
            _instance = new MyClass();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            // Reset state before each iteration (if method mutates state)
            _instance.Reset();
        }

        [Benchmark(Baseline = true)]
        public void MethodToOptimize_Baseline()
        {
            _instance.DoWork();
        }

        [Benchmark]
        public void MethodToOptimize_Optimized()
        {
            _instance.DoWorkOptimized();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            // One-time cleanup (runs once after all benchmarks)
            _instance.Dispose();
        }
    }
}
```

---

## Running Benchmarks

### Run All Benchmarks

```bash
cd BenchmarkSuite1
dotnet run -c Release
```

### Run Specific Benchmark Class

```bash
dotnet run -c Release -- --filter *GameEngineSimulationBenchmark*
```

### Run Specific Method

```bash
dotnet run -c Release -- --filter *SimulateTick_LargeVillage*
```

### Export Results

```bash
dotnet run -c Release -- --exporters json,html
```

Results will be saved in `BenchmarkSuite1/BenchmarkDotNet.Artifacts/results/`

---

## Best Practices

### 1. Use [GlobalSetup] for Expensive Initialization

```csharp
[GlobalSetup]
public void Setup()
{
    // Expensive operations like file loading, database setup
    // This runs ONCE before all benchmark iterations
    _engine = new GameEngine(config);
}
```

### 2. Use [IterationSetup] for State Reset

```csharp
[IterationSetup]
public void ResetState()
{
    // Reset mutable state between iterations
    // This ensures each iteration measures the same operation
    _engine.Reset();
}
```

**When to use:**
- Method mutates internal state
- Results depend on previous state
- Need consistent starting conditions

**When NOT to use:**
- Setup is expensive (use snapshots instead)
- Method is pure/stateless
- State doesn't affect performance

### 3. Use Snapshots for Complex State

Instead of recreating expensive objects:

```csharp
private class StateSnapshot
{
    public List<(Entity, Position)> EntityStates { get; set; } = new();
}

private StateSnapshot _snapshot = null!;

[GlobalSetup]
public void Setup()
{
    _engine = CreateEngine();  // Expensive
    _snapshot = TakeSnapshot(_engine);  // Save state
}

[IterationSetup]
public void ResetState()
{
    RestoreSnapshot(_engine, _snapshot);  // Fast restore
}
```

### 4. Mark Baseline Appropriately

```csharp
[Benchmark(Baseline = true)]  // The original, unoptimized version
public void Original() { ... }

[Benchmark]  // Optimized versions compared to baseline
public void Optimized() { ... }
```

**Baseline should be:**
- The smallest/simplest scenario (for scaling tests)
- The original implementation (for optimization tests)
- The most common use case (for comparison tests)

### 5. Use Meaningful Names

? **Good:**
```csharp
[Benchmark(Baseline = true)]
public void SimulateTick_SmallVillage_Original()

[Benchmark]
public void SimulateTick_SmallVillage_OptimizedTileClearing()
```

? **Bad:**
```csharp
[Benchmark]
public void Test1()

[Benchmark]
public void NewVersion()
```

### 6. Add CPU Diagnostics

```csharp
[SimpleJob(RuntimeMoniker.Net90)]
[CPUUsageDiagnoser]  // Enables detailed CPU profiling
public class MyBenchmark
{
    // ...
}
```

This shows which functions consume the most CPU time.

### 7. Avoid Common Pitfalls

? **Don't modify benchmark logic after first run:**
```csharp
// BAD - Changing the benchmark invalidates comparisons
[Benchmark]
public void MyMethod()
{
    DoWork();
    DoMoreWork();  // Added later - breaks historical comparison
}
```

? **Don't benchmark trivial operations:**
```csharp
// BAD - Too fast to measure accurately
[Benchmark]
public int AddTwoNumbers()
{
    return 1 + 2;  // Completes in nanoseconds, high variance
}
```

? **Don't include setup in benchmark:**
```csharp
// BAD - Measures setup, not the actual operation
[Benchmark]
public void ProcessData()
{
    var data = LoadLargeFile();  // Should be in [GlobalSetup]
    Process(data);
}
```

? **Do include warmup and proper iterations:**
```csharp
// BenchmarkDotNet handles this automatically
// Just ensure operations are long enough to measure (>1 ?s)
```

---

## Interpreting Results

### Sample Output

```
| Method                     | Mean      | Error    | StdDev   | Ratio | RatioSD |
|--------------------------- |----------:|---------:|---------:|------:|--------:|
| SimulateTick_SmallVillage  |  40.38 us | 1.738 us | 5.016 us |  1.00 |    0.00 |
| SimulateTick_MediumVillage |  95.76 us | 4.677 us | 13.57 us |  2.37 |    0.35 |
| SimulateTick_LargeVillage  | 178.69 us | 8.573 us | 24.87 us |  4.43 |    0.68 |
```

### Understanding Columns

- **Mean**: Average time per operation
- **Error**: Standard error (99.9% confidence interval)
- **StdDev**: Standard deviation (variance between runs)
- **Ratio**: Relative to baseline (baseline is always 1.00)
- **RatioSD**: Standard deviation of ratio

### What to Look For

**Good Results:**
- Low StdDev (<10% of Mean) = Consistent performance
- Ratio < 1.00 = Faster than baseline
- Scaling matches expectations

**Warning Signs:**
- High StdDev (>20% of Mean) = Inconsistent, needs investigation
- Ratio > 1.00 = Slower than baseline
- Non-linear scaling = Algorithmic problem

### Statistical Significance

BenchmarkDotNet automatically:
- Warms up the JIT compiler
- Runs multiple iterations
- Detects and reports outliers
- Calculates confidence intervals

**Rule of thumb:**
- Difference > 5% with low StdDev = Real improvement
- Difference < 5% = Noise, not meaningful
- Difference > 50% = Major optimization

---

## Troubleshooting

### High Variance (Large StdDev)

**Causes:**
- GC collections during benchmark
- Background processes
- Thermal throttling
- Inconsistent input data

**Solutions:**
```csharp
[MemoryDiagnoser]  // Track GC collections
[SimpleJob(warmupCount: 5, iterationCount: 15)]  // More iterations
```

Close background applications and run on AC power.

### Benchmark Too Fast

**Problem:** Operations complete in <1 ?s

**Solution:** Batch multiple operations
```csharp
[Benchmark]
public void BatchedOperation()
{
    for (int i = 0; i < 100; i++)
    {
        FastOperation();
    }
}
```

### Benchmark Too Slow

**Problem:** Single iteration takes >1 second

**Solution:** Use shorter job
```csharp
[SimpleJob(warmupCount: 1, iterationCount: 3)]
```

### Compilation Errors

**Problem:** Missing references

**Solution:** Ensure BenchmarkSuite1 references all required projects:
```xml
<ItemGroup>
  <ProjectReference Include="..\VillageBuilder.Engine\VillageBuilder.Engine.csproj" />
  <ProjectReference Include="..\VillageBuilder.Game\VillageBuilder.Game.csproj" />
</ItemGroup>
```

### Benchmarks Not Discovered

**Problem:** `run_benchmark` can't find benchmark

**Solution:** Check Program.cs uses `BenchmarkSwitcher`:
```csharp
var _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
```

Not:
```csharp
var _ = BenchmarkRunner.Run<SpecificBenchmark>();  // Only runs one class
```

---

## Advanced Topics

### Memory Diagnostics

```csharp
[MemoryDiagnoser]
public class MyBenchmark
{
    // Results include:
    // - Gen0: Collections
    // - Gen1: Collections  
    // - Gen2: Collections
    // - Allocated: Bytes per operation
}
```

### Custom Job Configuration

```csharp
[SimpleJob(
    RuntimeMoniker.Net90,
    warmupCount: 3,
    iterationCount: 10,
    invocationCount: 1000
)]
```

### Parameterized Benchmarks

```csharp
[Params(10, 100, 1000)]
public int Size;

[Benchmark]
public void ProcessData()
{
    ProcessCollection(Size);
}
```

Results show performance for each parameter value.

---

## Checklist for New Benchmarks

Before committing a new benchmark:

- [ ] Benchmark class has `[SimpleJob]` and `[CPUUsageDiagnoser]` attributes
- [ ] Has `[GlobalSetup]` for one-time initialization
- [ ] Uses `[IterationSetup]` if method mutates state
- [ ] Has at least one `[Benchmark(Baseline = true)]` method
- [ ] Benchmark names are descriptive and consistent
- [ ] Runs successfully in Release mode
- [ ] StdDev is <20% of Mean (low variance)
- [ ] Operation takes >1 ?s to complete (measurable)
- [ ] Added to documentation in this file
- [ ] Results documented in PERFORMANCE_OPTIMIZATIONS.md

---

## Resources

### Official Documentation
- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/articles/overview.html)
- [BenchmarkDotNet Samples](https://github.com/dotnet/BenchmarkDotNet/tree/master/samples)

### Related Documentation
- [PERFORMANCE_OPTIMIZATIONS.md](./PERFORMANCE_OPTIMIZATIONS.md) - Optimization results
- [PROFILING_WORKFLOW.md](./PROFILING_WORKFLOW.md) - Profiling guide (if exists)

### Support
For questions or issues with benchmarks, refer to the BenchmarkDotNet GitHub repository or create an issue in the VillageBuilder repository.

---

## Changelog

### 2024-01-XX - Initial Benchmark Infrastructure
- Created BenchmarkSuite1 project
- Added GameEngineSimulationBenchmark
- Added TextRenderingBenchmark
- Established baseline measurements

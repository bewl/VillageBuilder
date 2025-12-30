# How to Run Improved Benchmarks - Step by Step Guide

## What We Changed

### 1. Added Memory Diagnostics ?
```csharp
[MemoryDiagnoser]  // Track GC collections and memory allocations
```

**Shows:**
- Gen0/Gen1/Gen2 garbage collections
- Memory allocated per operation
- Helps identify if GC is causing variance

### 2. Increased Iterations ?
```csharp
[SimpleJob(
    RuntimeMoniker.Net90,
    warmupCount: 5,      // Was: default (3)
    iterationCount: 20   // Was: default (15)
)]
```

**Benefits:**
- More stable mean calculation
- Better statistical confidence
- Reduced measurement noise

---

## How to Run Benchmarks Properly

### Step 1: Prepare Your System ??

**CRITICAL - Do these BEFORE running:**

1. **Close ALL background applications:**
   ```
   ? Close: Chrome/Edge/Firefox
   ? Close: Visual Studio (run from command line)
   ? Close: Discord, Slack, Teams
   ? Close: Steam, Epic Games
   ? Disable: Windows Defender (temporarily)
   ? Close: Any other apps
   ```

2. **Plug in laptop to AC power:**
   - Running on battery = thermal throttling
   - CPU will slow down to save power
   - Results will be inconsistent

3. **Let system cool down:**
   - Wait 5 minutes after closing apps
   - CPU temperature affects performance
   - Thermal throttling = variance

### Step 2: Run from Command Line ?

**DON'T run from Visual Studio** (it consumes CPU/memory)

```powershell
# Navigate to benchmark project
cd D:\MyRepo\VillageBuilder\BenchmarkSuite1

# Run benchmarks (this will take ~5-10 minutes)
dotnet run -c Release
```

**Expected output:**
```
// * Summary *

BenchmarkDotNet=v0.15.2, OS=Windows 11
Intel Core i7-... CPU 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  .NET 9.0 : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=.NET 9.0  Runtime=.NET 9.0  IterationCount=20  WarmupCount=5

| Method                        | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated |
|-------------------------------|----------|----------|----------|-------|--------|-----------|
| SimulateTick_SmallVillage     | 42.38 us | 0.738 us | 0.690 us | 1.00  | 0.1221 | 2.1 KB    |
| SimulateTick_MediumVillage    | 95.76 us | 1.877 us | 1.755 us | 2.26  | 0.3662 | 6.3 KB    |
| SimulateTick_LargeVillage     | 178.2 us | 3.201 us | 2.994 us | 4.20  | 0.7324 | 12.6 KB   |
| SimulateTick_VeryLargeVillage | 3542 us  | 67.34 us | 62.98 us | 83.6  | 5.8594 | 100 KB    |
```

### Step 3: Interpret Results ??

#### Good Results Look Like This: ?
```
Mean:     42.38 us
Error:    ±0.738 us   (1.7% of Mean)  ? LOW variance!
StdDev:   0.690 us    (1.6% of Mean)  ? Consistent!
Gen0:     0.1221      ? Few GC collections
Allocated: 2.1 KB     ? Low memory pressure
```

**Characteristics of reliable results:**
- ? Error < 5% of Mean
- ? StdDev < 5% of Mean
- ? Clear scaling pattern (e.g., 1.00 ? 2.26 ? 4.20)
- ? No overlapping confidence intervals

#### Bad Results Look Like This: ?
```
Mean:     42.38 us
Error:    ±18.2 us    (43% of Mean)  ? HIGH variance!
StdDev:   15.6 us     (37% of Mean)  ? Inconsistent!
Gen0:     2.5000      ? Many GC collections
Allocated: 45 KB      ? High memory pressure
```

**Characteristics of unreliable results:**
- ? Error > 10% of Mean
- ? StdDev > 10% of Mean
- ? No clear pattern
- ? Overlapping confidence intervals

---

## Understanding New Columns

### Memory Columns (NEW!)

| Column | Meaning | Example | Good/Bad |
|--------|---------|---------|----------|
| **Gen0** | Gen0 GC collections per 1000 ops | 0.1221 | ? < 1.0 is good |
| **Gen1** | Gen1 GC collections per 1000 ops | 0.0122 | ? < 0.1 is good |
| **Gen2** | Gen2 GC collections per 1000 ops | 0.0000 | ? 0 is ideal |
| **Allocated** | Memory allocated per operation | 2.1 KB | ? < 10 KB is good |

### What High GC Means ??

```
Gen0: 5.0000  ? BAD! 5 collections per 1000 operations
```

**Problem:** GC pausing execution frequently
**Solution:** Reduce allocations in SimulateTick()

### What High Memory Allocation Means ??

```
Allocated: 150 KB  ? BAD! Lots of memory pressure
```

**Problem:** Creating too many temporary objects
**Solution:** Object pooling, reuse collections

---

## Troubleshooting

### Problem: Still High Variance After Following Steps

**Symptoms:**
```
Error: ±15 us (30% of Mean)
```

**Solutions:**

1. **Check if background processes are running:**
   ```powershell
   # Open Task Manager (Ctrl+Shift+Esc)
   # Sort by CPU usage
   # Kill any high CPU processes
   ```

2. **Increase iterations even more:**
   ```csharp
   [SimpleJob(
       RuntimeMoniker.Net90,
       warmupCount: 10,     // Double warmup
       iterationCount: 30   // More iterations
   )]
   ```

3. **Pin to single CPU core:**
   ```csharp
   [SimpleJob(RuntimeMoniker.Net90, affinity: 1)]
   ```

4. **Disable CPU frequency scaling (advanced):**
   - Windows Power Options ? High Performance
   - Disable "Processor power management"

### Problem: Benchmarks Take Too Long

**Current time:** ~5-10 minutes with 20 iterations

**If too slow:**
```csharp
[SimpleJob(
    RuntimeMoniker.Net90,
    warmupCount: 3,
    iterationCount: 10  // Reduce to 10 (minimum recommended)
)]
```

### Problem: Out of Memory

**VeryLarge scenario uses a lot of memory (600 people + 1000 buildings)**

**Solutions:**
- Reduce VeryLarge scenario size:
  ```csharp
  _engineVeryLarge = CreateEngine(100, 500); // Half the size
  ```
- Or remove VeryLarge benchmark entirely

---

## Expected Performance Characteristics

### What Good Scaling Looks Like ?

```
Small:    1.00x (baseline)
Medium:   2.0-2.5x
Large:    4.0-5.0x
VeryLarge: 80-100x
```

This is **O(n)** or **sub-linear** scaling - excellent!

### What Bad Scaling Looks Like ?

```
Small:    1.00x (baseline)
Medium:   10x    ? BAD! Should be ~2x
Large:    100x   ? BAD! Should be ~4x
VeryLarge: 10000x ? BAD! Should be ~100x
```

This is **O(n²)** or worse - indicates nested loops over all entities.

---

## Performance Targets

### Current Targets (Estimated)

Based on your previous results (~4-5 ms per tick):

| Village Size | Target Mean | Target Error | Target Gen0 |
|--------------|-------------|--------------|-------------|
| **Small**    | 40-60 ?s    | < 5 ?s       | < 0.5       |
| **Medium**   | 80-120 ?s   | < 10 ?s      | < 1.0       |
| **Large**    | 150-250 ?s  | < 20 ?s      | < 2.0       |
| **VeryLarge**| 3-6 ms      | < 500 ?s     | < 10.0      |

### If You Beat These Targets ?

**Congratulations!** Your simulation is highly optimized.

### If You Miss These Targets ?

**Investigate:**
1. Check Memory Diagnostics for high allocations
2. Profile with dotnet-trace or Visual Studio Profiler
3. Look for O(n²) patterns in code
4. Check if HashSet optimization is actually being used

---

## After Running: What to Do With Results

### 1. Document Performance

Save results to `Documentation/Performance/BENCHMARK_RESULTS_2024.md`:

```markdown
# Benchmark Results - Date

## System Info
- CPU: Intel Core i7-...
- RAM: 16 GB
- OS: Windows 11
- .NET: 9.0

## Results

| Method | Mean | Error | Ratio | Gen0 | Allocated |
|--------|------|-------|-------|------|-----------|
| Small  | 42 ?s | ±0.7 ?s | 1.00 | 0.12 | 2.1 KB |
| ...

## Analysis
- Scaling: Sub-linear ?
- Memory pressure: Low ?
- GC impact: Minimal ?
```

### 2. Compare to Previous Results

Track performance over time:
- Did optimization improve results?
- Did new feature slow things down?
- Are we still meeting targets?

### 3. Profile If Needed

If results are worse than expected:
```powershell
# Run with profiler
dotnet trace collect -- dotnet run -c Release

# Analyze trace
dotnet trace analyze trace.nettrace
```

---

## Quick Checklist Before Running

- [ ] Closed all background apps
- [ ] Plugged in laptop to AC power
- [ ] Let system cool for 5 minutes
- [ ] Running from command line (not VS)
- [ ] In Release mode (not Debug)
- [ ] Have 10-15 minutes available
- [ ] Task Manager shows low CPU usage

**Ready? Run the benchmark!**

```powershell
cd D:\MyRepo\VillageBuilder\BenchmarkSuite1
dotnet run -c Release
```

---

## Understanding Your Previous Results

### Why They Were Unreliable

```
Mean: 4.122 ms ± 1.886 ms (±46% variance)
```

**Problems:**
- ? Error was 46% of Mean (target: <5%)
- ? Confidence intervals overlapped
- ? Large claimed to be faster than Small (impossible)
- ? No memory diagnostics

**Likely causes:**
- Background processes consuming CPU
- GC collections at random times
- Thermal throttling
- Too few iterations

### What to Expect Now

With our changes, you should see:

```
Mean: 42.38 ?s ± 0.738 ?s (±1.7% variance)
```

**Improvements:**
- ? Error is <2% of Mean
- ? Clear scaling pattern
- ? Memory diagnostics available
- ? Reliable, reproducible results

---

**Good luck with your benchmarks!** ??

Post the new results when you're done, and we can analyze the performance characteristics!

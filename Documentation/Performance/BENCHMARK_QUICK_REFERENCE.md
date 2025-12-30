# Benchmark Quick Reference Card

## ? Quick Start

```powershell
# 1. Close all apps
# 2. Plug in laptop
# 3. Run from command line
cd D:\MyRepo\VillageBuilder\BenchmarkSuite1
dotnet run -c Release
```

---

## ?? Reading Results

### Column Guide

| Column | What It Means |
|--------|---------------|
| **Mean** | Average time per operation |
| **Error** | ±Confidence interval (99.9%) |
| **StdDev** | How much results vary |
| **Ratio** | Compared to baseline (Small = 1.00) |
| **Gen0** | Garbage collections (per 1000 ops) |
| **Allocated** | Memory used per operation |

### Good vs Bad Results

? **GOOD**
```
Mean:  42.38 ?s
Error: ±0.738 ?s  (1.7% of Mean)
Gen0:  0.1221
```

? **BAD**
```
Mean:  42.38 ?s
Error: ±18.2 ?s   (43% of Mean)
Gen0:  5.0000
```

---

## ?? Target Values

| Metric | Good | Warning | Bad |
|--------|------|---------|-----|
| **Error/Mean** | <5% | 5-10% | >10% |
| **Gen0** | <1.0 | 1.0-3.0 | >3.0 |
| **Scaling** | 1?2?4?80 | 1?3?9?200 | 1?10?100?10000 |

---

## ?? Before Running

- [ ] **Close:** Browser, VS, Discord, Steam
- [ ] **Plug in:** AC power (no battery!)
- [ ] **Wait:** 5 min for CPU to cool
- [ ] **Check:** Task Manager shows low CPU
- [ ] **Mode:** Release (not Debug!)
- [ ] **Time:** Have 10 minutes available

---

## ?? If Results Are Bad

### High Variance (Error >10% of Mean)
1. Close more background apps
2. Wait longer for CPU to cool
3. Increase iterations to 30

### High GC (Gen0 >3.0)
1. Check for memory leaks
2. Profile with `dotnet trace`
3. Reduce allocations in SimulateTick()

### Poor Scaling (VeryLarge >200x)
1. Look for nested loops
2. Check if HashSet optimization is used
3. Profile hotspots

---

## ?? Expected Results

```
Small:     ~40-60 ?s   (baseline)
Medium:    ~80-120 ?s  (2x)
Large:     ~150-250 ?s (4x)
VeryLarge: ~3-6 ms     (80x)
```

---

## ?? Warning Signs

- ? Error >30% of Mean ? System too busy
- ? Large faster than Small ? Unreliable data
- ? Gen2 collections ? Memory pressure
- ? Allocated >50 KB ? Too many allocations

---

## ? Success Indicators

- ? Error <5% of Mean
- ? Clear scaling pattern (2x, 4x, etc.)
- ? No Gen2 collections
- ? Low memory allocations
- ? Reproducible results

---

## ?? Save Results

```markdown
# Benchmark Results - 2024-XX-XX

## System
- CPU: [Your CPU]
- .NET: 9.0

## Results
[Paste table here]

## Analysis
- Scaling: [Linear/Sub-linear/Quadratic]
- Memory: [Low/Medium/High]
- GC Impact: [Minimal/Moderate/High]
```

Save to: `Documentation/Performance/BENCHMARK_RESULTS_2024-XX-XX.md`

---

## ?? Help

**Still seeing high variance?**
- See full guide: `Documentation/Performance/HOW_TO_RUN_BENCHMARKS.md`
- Check CPU temperature
- Try different time of day
- Restart computer

**Results look good?**
- Document them!
- Compare to previous runs
- Celebrate optimization success! ??

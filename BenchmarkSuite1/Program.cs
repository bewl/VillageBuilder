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

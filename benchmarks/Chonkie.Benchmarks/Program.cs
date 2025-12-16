using BenchmarkDotNet.Running;

namespace Chonkie.Benchmarks;

/// <summary>
/// Main entry point for running benchmarks.
/// </summary>
public class Program
{
    /// <summary>
    /// Runs all benchmarks in the assembly.
    /// </summary>
    /// <param name="args">Command line arguments for BenchmarkDotNet.</param>
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

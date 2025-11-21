using BenchmarkDotNet.Running;

namespace RotateImageBenchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<RotateRgb24Benchmarks>();
        }
    }
}
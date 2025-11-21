using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters.Csv;

namespace RotateImageBenchmarks
{
    // Custom configuration for consistent, richer output.
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            int iterationCount = 10;
            int warmupCount = 3;

            AddJob(Job
                .Default
                .WithRuntime(CoreRuntime.Core10_0)
                .WithId(".NET 10")
                .WithWarmupCount(warmupCount)
                .WithIterationCount(iterationCount)
                .WithLaunchCount(1));

            AddJob(Job
                .Default
                .WithRuntime(CoreRuntime.Core90)
                .WithId(".NET 9")
                .WithWarmupCount(warmupCount)
                .WithIterationCount(iterationCount)
                .WithLaunchCount(1));

            AddJob(Job
                .Default
                .WithRuntime(CoreRuntime.Core80)
                .WithId(".NET 8")
                .WithWarmupCount(warmupCount)
                .WithIterationCount(iterationCount)
                .WithLaunchCount(1));

            AddDiagnoser(MemoryDiagnoser.Default);
            AddExporter(MarkdownExporter.Default, HtmlExporter.Default);
            AddColumn(TargetMethodColumn.Method, StatisticColumn.Mean);
        }
    }
}
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
            AddJob(Job
                .Default
                .WithRuntime(CoreRuntime.Core10_0)
                .WithId(".NET 10")
                .WithWarmupCount(3)
                .WithIterationCount(5)
                .WithLaunchCount(1));

            AddJob(Job
                .Default
                .WithRuntime(CoreRuntime.Core90)
                .WithId(".NET 9")
                .WithWarmupCount(3)
                .WithIterationCount(5)
                .WithLaunchCount(1));

            AddJob(Job
                .Default
                .WithRuntime(CoreRuntime.Core80)
                .WithId(".NET 8")
                .WithWarmupCount(3)
                .WithIterationCount(5)
                .WithLaunchCount(1));

            AddDiagnoser(MemoryDiagnoser.Default);
            AddExporter(MarkdownExporter.Default, HtmlExporter.Default, CsvExporter.Default);
            AddColumn(TargetMethodColumn.Method, StatisticColumn.Mean, StatisticColumn.Median,
                      StatisticColumn.StdDev, StatisticColumn.P95, StatisticColumn.OperationsPerSecond);
        }
    }
}
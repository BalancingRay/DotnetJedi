using BenchmarkDotNet.Attributes;
using RotateImage;

namespace RotateImageBenchmarks
{
    // Benchmark groups for 3bpp rotation methods.
    // Allocation done once per (width,height,repeatedCount) combo unless explicitly benchmarked.
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser]
    public class RotateRgb24Benchmarks
    {
        // Dimensions to test (mirrors original test + can extend).
        [Params(1024)]
        public int Width { get; set; }

        [Params(600, 1920)]
        public int Height { get; set; }

        private byte[] _input = default!;
        private byte[] _output = default!;

        [GlobalSetup]
        public void Setup()
        {
            _input = new byte[Width * Height * 3];
            _output = new byte[_input.Length];
            Random.Shared.NextBytes(_input);
        }

        [Benchmark(Baseline = true)]
        public void Rotate90_CopyBlock_MinTemps()
        {
            var input = _input;
            var output = _output;
            RotationUtils.Rotate90ClockwiseRgb24_CopyBlock_MinTemps(input, output, Width, Height);

        }

        [Benchmark]
        public void Rotate_Generic_ToBpp3()
        {
            var input = _input;
            var output = _output;
            RotationUtils.RotateToBpp3(input, output, Width, Height);
        }

        [Benchmark]
        public void Rotate_Generic_ToBpp3_AsSpan()
        {
            var input = _input;
            var output = _output;
            RotationUtils.RotateToBpp3_AsSpan(input, output, Width, Height);
        }
    }
}
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

        public enum ImageSize
        {
            ShortFrame_10_1024,
            LargeFrame_100_1024,
            FullHD_1920_1080,
            FourK_3840_2160
        }

        [Params( ImageSize.ShortFrame_10_1024,
                 ImageSize.LargeFrame_100_1024,
                 ImageSize.FullHD_1920_1080,
                 ImageSize.FourK_3840_2160)]
        public ImageSize Size { get; set; }

        public int Width => Size switch
        {
            ImageSize.ShortFrame_10_1024 => 1024,
            ImageSize.LargeFrame_100_1024 => 1024,
            ImageSize.FullHD_1920_1080 => 1920,
            ImageSize.FourK_3840_2160 => 3840,
            _ => throw new ArgumentOutOfRangeException()
        };

        public int Height => Size switch
        {
            ImageSize.ShortFrame_10_1024 => 10,
            ImageSize.LargeFrame_100_1024 => 100,
            ImageSize.FullHD_1920_1080 => 1080,
            ImageSize.FourK_3840_2160 => 2160,
            _ => throw new ArgumentOutOfRangeException()
        };

        private byte[] _input = default!;
        private byte[] _output = default!;

        [GlobalSetup]
        public void Setup()
        {
            _input = new byte[Width * Height * 3];
            _output = new byte[_input.Length];
            Random.Shared.NextBytes(_input);
        }

        [Benchmark(Baseline = true)] // Method used as baseline to compare with different implemenetaiton for each of iput paramters
        public void Rotate90_CopyBlock()
        {
            var input = _input;
            var output = _output;
            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(input, output, Width, Height);
        }

        [Benchmark]
        public void Rotate_ToBpp3()
        {
            var input = _input;
            var output = _output;
            RotationUtils.RotateToBpp3(input, output, Width, Height);
        }

        [Benchmark]
        public void Rotate_ToBpp3_AsSpan()
        {
            var input = _input;
            var output = _output;
            RotationUtils.RotateToBpp3_AsSpan(input, output, Width, Height);
        }

        [Benchmark]
        public void Rotate_ToBpp3_Tiles()
        {
            var input = _input;
            var output = _output;
            RotationUtils.Rotate90ClockwiseRgb24_Tiled(input, output, Width, Height);
        }

        [Benchmark]
        public void RotateToBpp3_Unsafe_SSE41()
        {
            var input = _input;
            var output = _output;
            RotationUtils.RotateToBpp3_Unsafe_SSE41(input, output, Width, Height);
        }

        [Benchmark]
        public void RotateToBpp3_Unsafe_Parallel()
        {
            var input = _input;
            var output = _output;
            RotationUtils.RotateToBpp3_Unsafe_Parallel(input, output, Width, Height);
        }

        [Benchmark]
        public void RotateToBpp3_Unsafe_Parallel_SSSE3()
        {
            var input = _input;
            var output = _output;
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSSE3(input, output, Width, Height);
        }

        [Benchmark]
        public void RotateToBpp3_Unsafe_Parallel_SSE41()
        {
            var input = _input;
            var output = _output;
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41(input, output, Width, Height);
        }

        [Benchmark]
        public void RotateToBpp3_Unsafe_Parallel_SSE41_Native()
        {
            var input = _input;
            var output = _output;
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41_Native(input, output, Width, Height);
        }
    }
}
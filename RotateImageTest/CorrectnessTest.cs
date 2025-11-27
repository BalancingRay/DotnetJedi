using RotateImage;
namespace RotateImageTest
{
    [TestFixture]
    [Parallelizable(scope: ParallelScope.All)]
    public class ImageOrientationUtilsTests
    {
        // Test data: 2x3 image, 3 bytes per pixel (RGB)
        // Layout before rotation (row-major, left-to-right, top-to-bottom):
        // [ R0 G0 B0 | R1 G1 B1 ]      y=0 (row 0)
        // [ R2 G2 B2 | R3 G3 B3 ]      y=1 (row 1)
        // [ R4 G4 B4 | R5 G5 B5 ]      y=2 (row 2)
        static readonly byte[] Input =
        {
        0, 1, 2,   // pixel (0,0)
        3, 4, 5,   // pixel (1,0)
        6, 7, 8,   // pixel (0,1)
        9,10,11,   // pixel (1,1)
        12,13,14,  // pixel (0,2)
        15,16,17   // pixel (1,2)
    };
        // After rotate 90° CW, should become a 3x2 image, pixels:
        // [ R4 G4 B4 | R2 G2 B2 | R0 G0 B0 ]   // y=0 (row 0)
        // [ R5 G5 B5 | R3 G3 B3 | R1 G1 B1 ]   // y=1 (row 1)
        static readonly byte[] ExpectedRotated =
        {
        12,13,14,  6,7,8,  0,1,2,
        15,16,17,  9,10,11, 3,4,5
    };

        static readonly int Width = 2;
        static readonly int Height = 3;
        static readonly int BytesPerPixel = 3;

        // Helper: create ArraySegment from input
        ArraySegment<byte> InputSegment() => new(Input, 0, Input.Length);

        [Test]
        public void Rotate90_CorrectnessOnEdgeShapes()
        {
            var inp = new byte[] { 42, 99, 100 };
            var expected = new byte[] { 42, 99, 100 };
            var out1 = new byte[expected.Length];
            RotationUtils.RotateToBpp3_AsSpan(inp, out1, 1, 1);
            Assert.That(out1, Is.EqualTo(expected));
        }

        [Test]
        public void Rotate90_CopyBlock_WorksCorrectly()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(intput, result, Width, Height);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }

        [Test]
        public void Rotate90_RotateToBpp3_WorksCorrectly()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.RotateToBpp3(intput, result, Width, Height);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }

        [Test]
        public void Rotate90_TwoPass_WorksCorrectly()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.Rotate90ClockwiseRgb24_TwoPassTransposeThenFlip(intput, result, Width, Height);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }

        [Test]
        public void Rotate90_Tiled_WorksCorrectly()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.Rotate90ClockwiseRgb24_Tiled(intput, result, Width, Height);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }

        [Test]
        public void Rotate90_Tiled32_WorksCorrectly()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.Rotate90ClockwiseRgb24_Tiled(intput, result, Width, Height,32);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }

        [Test]
        public void Rotate90_Tiled31_WorksCorrectly()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.Rotate90ClockwiseRgb24_Tiled(intput, result, Width, Height,31);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }

        [Test]
        public void Rotate90_CopyToNew_WorksCorrectly()
        {
            var intput = InputSegment();
            byte[] result = new byte[ExpectedRotated.Length];
            RotationUtils.RotateToBpp3_AsSpan(intput, result, Width, Height);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }      

        [Test]
        public void RotateToBpp3_Unsafe_SSE41()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.RotateToBpp3_Unsafe_SSE41(intput, result, Width, Height);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }

        [Test]
        public void RotateToBpp3_Unsafe_Parallel()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.RotateToBpp3_Unsafe_Parallel(intput, result, Width, Height);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }

        [Test]
        public void RotateToBpp3_Unsafe_Parallel_SSSE3()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSSE3(intput, result, Width, Height);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }

        [Test]
        public void RotateToBpp3_Unsafe_Parallel_SSE41()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.RotateToBpp3_Unsafe_SSE41(intput, result, Width, Height);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }

        [Test]
        public void RotateToBpp3_Unsafe_Parallel_SSE41_Native()
        {
            var intput = InputSegment();
            var result = new byte[ExpectedRotated.Length];
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41_Native(intput, result, Width, Height);
            Assert.That(result, Is.EqualTo(ExpectedRotated));
        }        
    }
}
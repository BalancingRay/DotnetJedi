using RotateImage;

namespace RotateImageTest
{
    public class MultirotationTests
    {
        #region Four times Rotation is Original
        [TestCase(2, 3)]
        [TestCase(80,60)]
        [TestCase(102,555)]
        [TestCase(387,276)]
        public void Test_Rotate_3bpp_CopyBlock_MinTemps(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(input, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(t1, t2, height, width);

            Assert.That(t2, Is.Not.EqualTo(input));

            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(t2, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(input));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Rotate_3bpp(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3(input, t1, width, height);
            RotationUtils.RotateToBpp3(t1, t2, height, width);

            Assert.That(t2, Is.Not.EqualTo(input));

            RotationUtils.RotateToBpp3(t2, t1, width, height);
            RotationUtils.RotateToBpp3(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(input));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Rotate_Unsafe_Parallel_SSSE3(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSSE3(input, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSSE3(t1, t2, height, width);

            Assert.That(t2, Is.Not.EqualTo(input));

            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSSE3(t2, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSSE3(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(input));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Rotate_3bpp_Unsafe_Parallel_SSE41(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41(input, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41(t1, t2, height, width);

            Assert.That(t2, Is.Not.EqualTo(input));

            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41(t2, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(input));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Rotate_3bpp_Tiled(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate90ClockwiseRgb24_Tiled(input, t1, width, height);
            RotationUtils.Rotate90ClockwiseRgb24_Tiled(t1, t2, height, width);

            Assert.That(t2, Is.Not.EqualTo(input));

            RotationUtils.Rotate90ClockwiseRgb24_Tiled(t2, t1, width, height);
            RotationUtils.Rotate90ClockwiseRgb24_Tiled(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(input));
        }

        

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Rotate_3bpp_CopyBlock_Tiled_64(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(input, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(t1, t2, height, width);

            Assert.That(t2, Is.Not.EqualTo(input));

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(t2, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(input));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Rotate_3bpp_CopyBlock_Tiled_32(int width, int height)
        {
            int tileSize = 32;
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(input, t1, width, height, tileSize);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(t1, t2, height, width, tileSize);

            Assert.That(t2, Is.Not.EqualTo(input));

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(t2, t1, width, height, tileSize);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(t1, t2, height, width, tileSize);

            Assert.That(t2, Is.EqualTo(input));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Rotate_3bpp_Unsafe_SSE41(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3_Unsafe_SSE41(input, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_SSE41(t1, t2, height, width);

            Assert.That(t2, Is.Not.EqualTo(input));

            RotationUtils.RotateToBpp3_Unsafe_SSE41(t2, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_SSE41(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(input));
        }
        #endregion Four Rotations is Original
    }
}
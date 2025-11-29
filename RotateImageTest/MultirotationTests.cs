using RotateImage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RotateImageTest
{
    public class MultirotationTests
    {
        #region Four times Rotation is Original
        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_4Rotate_3bpp_CopyBlock_MinTemps(int width, int height)
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
        public void Test_4Rotate_3bpp(int width, int height)
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
        public void Test_4Rotate_3bpp_Unsafe_Parallel_SSE41(int width, int height)
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
        public void Test_4Rotate_3bpp_Tiled(int width, int height)
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
        public void Test_4Rotate_3bpp_CopyBlock_Tiled_64(int width, int height)
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
        public void Test_4Rotate_3bpp_CopyBlock_Tiled_32(int width, int height)
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
        public void Test_4Rotate_3bpp_CopyBlock_Tiled_Ssse3_3(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled_vector256(input, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled_vector256(t1, t2, height, width);

            Assert.That(t2, Is.Not.EqualTo(input));

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled_vector256(t2, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled_vector256(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(input));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_4Rotate_3bpp_Unsafe_SSE41(int width, int height)
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

        #region Gray Two times Rotation is Reverse bytes


        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Gray2Rotate_3bpp_CopyBlock_MinTemps(int width, int height)
        {
            var input = new byte[width * height * 3];
            SetRandomGray(input);
            var output = GetRotate180Gray(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(input, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Gray2Rotate_3bpp(int width, int height)
        {
            var input = new byte[width * height * 3];
            SetRandomGray(input);
            var output = GetRotate180Gray(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3(input, t1, width, height);
            RotationUtils.RotateToBpp3(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Gray2Rotate_3bpp_Unsafe_Parallel_SSE41(int width, int height)
        {
            var input = new byte[width * height * 3];
            SetRandomGray(input);
            var output = GetRotate180Gray(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41(input, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Gray2Rotate_3bpp_Tiled(int width, int height)
        {
            var input = new byte[width * height * 3];
            SetRandomGray(input);
            var output = GetRotate180Gray(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate90ClockwiseRgb24_Tiled(input, t1, width, height);
            RotationUtils.Rotate90ClockwiseRgb24_Tiled(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Gray2Rotate_3bpp_CopyBlock_Tiled_64(int width, int height)
        {
            var input = new byte[width * height * 3];
            SetRandomGray(input);
            var output = GetRotate180Gray(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(input, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Gray2Rotate_3bpp_CopyBlock_Tiled_32(int width, int height)
        {
            int tileSize = 32;
            var input = new byte[width * height * 3];
            SetRandomGray(input);
            var output = GetRotate180Gray(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(input, t1, width, height, tileSize);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(t1, t2, height, width, tileSize);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_Gray2Rotate_3bpp_Unsafe_SSE41(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3_Unsafe_SSE41(input, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_SSE41(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp_CopyBlock_MinTemps(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(input, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp_CopyBlock_MinTemps_Stackalloc(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps_Stackalloc(input, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_MinTemps_Stackalloc(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3(input, t1, width, height);
            RotationUtils.RotateToBpp3(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp_Unsafe_Parallel_SSE41(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41(input, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp_Tiled(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate90ClockwiseRgb24_Tiled(input, t1, width, height);
            RotationUtils.Rotate90ClockwiseRgb24_Tiled(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp_CopyBlock_Tiled_64(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(input, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp_CopyBlock_TiledV2_64(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled_4copy(input, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled_4copy(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(3, 3)]
        [TestCase(3, 6)]
        [TestCase(6, 3)]
        [TestCase(9, 6)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp_CopyBlock_Tiled_Ssse3_3(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled_vector256(input, t1, width, height);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled_vector256(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp_CopyBlock_Tiled_32(int width, int height)
        {
            int tileSize = 32;
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(input, t1, width, height, tileSize);
            RotationUtils.Rotate_3bpp_CopyBlock_Tiled(t1, t2, height, width, tileSize);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(3, 3)]
        [TestCase(3, 6)]
        [TestCase(6, 3)]
        [TestCase(9, 6)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp_Unsafe_SSE41(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3_Unsafe_SSE41(input, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_SSE41(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        [TestCase(2, 3)]
        [TestCase(3, 3)]
        [TestCase(3, 6)]
        [TestCase(6, 3)]
        [TestCase(9, 6)]
        [TestCase(80, 60)]
        [TestCase(102, 555)]
        [TestCase(387, 276)]
        public void Test_2Rotate_3bpp_Unsafe_Parallel_SSE41_Native(int width, int height)
        {
            var input = new byte[width * height * 3];
            Random.Shared.NextBytes(input);
            var output = GetRotate180rgb(input);

            var t1 = new byte[input.Length];
            var t2 = new byte[input.Length];

            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41_Native(input, t1, width, height);
            RotationUtils.RotateToBpp3_Unsafe_Parallel_SSE41_Native(t1, t2, height, width);

            Assert.That(t2, Is.EqualTo(output));
        }

        void SetRandomGray(byte[] data)
        {
            var random = new Random();
            for (var i = 0; i < data.Length; i += 3)
            {
                byte value = (byte)random.Next(255);
                data[i] = value;
                data[i + 1] = value;
                data[i + 2] = value;
            }
        }

        byte[] GetRotate180Gray(byte[] input) => input.Reverse().ToArray();

        byte[] GetRotate180rgb(byte[] input)
        {
            var output = input.Reverse().ToArray();
            for (var i = 0; i < output.Length; i += 3)
            {
                byte t = output[i];
                output[i] = output[i + 2];
                output[i + 2] = t;
            }
            return output;
        }

        #endregion
    }
}
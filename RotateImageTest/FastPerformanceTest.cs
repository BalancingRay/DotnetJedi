using RotateImage;

namespace RotateImageTest
{
    public class FastPerformanceTest
    {
        const int repeatedCount = 50;

        [TestCase(800,600, repeatedCount)]
        [TestCase(1024,1920, repeatedCount)]
        [TestCase(2000,4000, repeatedCount)]
        public void Test_Rotate_3bpp_CopyBlock_MinTemps(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for(int i = 0; i < repeatedCount; i++) {
                RotationUtils.Rotate_3bpp_CopyBlock_MinTemps(input, output, width, height);
            }
        }

        [TestCase(800, 600, repeatedCount)]
        [TestCase(1024, 1920, repeatedCount)]
        [TestCase(2000, 4000, repeatedCount)]
        public void Test_RotateToBpp3(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for (int i = 0; i < repeatedCount; i++)
            {
                RotationUtils.RotateToBpp3(input, output, width, height);
            }
        }

        [TestCase(800, 600, repeatedCount)]
        [TestCase(1024, 1920, repeatedCount)]
        [TestCase(2000, 4000, repeatedCount)]
        public void Test_RotateToBpp3_AsSpan(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for (int i = 0; i < repeatedCount; i++)
            {
                RotationUtils.RotateToBpp3_AsSpan(input, output, width, height);
            }
        }

        [TestCase(800, 600, repeatedCount)]
        [TestCase(1024, 1920, repeatedCount)]
        [TestCase(2000, 4000, repeatedCount)]
        public void Test_Rotate90_3bpp_Tiled128(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for (int i = 0; i < repeatedCount; i++)
            {
                RotationUtils.Rotate90ClockwiseRgb24_Tiled(input, output, width, height, 128);
            }
        }

        [TestCase(800, 600, repeatedCount)]
        [TestCase(1024, 1920, repeatedCount)]
        [TestCase(2000, 4000, repeatedCount)]
        public void Test_Rotate90_3bpp_Tiles_Ssse3_x3(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for (int i = 0; i < repeatedCount; i++)
            {
                RotationUtils.Rotate_3bpp_CopyBlock_Tiled_vector256(input, output, width, height);
            }
        }

        [TestCase(800, 600, repeatedCount)]
        [TestCase(1024, 1920, repeatedCount)]
        [TestCase(2000, 4000, repeatedCount)]
        public void Test_Rotate90_3bpp_v2(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for (int i = 0; i < repeatedCount; i++)
            {
                RotationUtils.Rotate_3bpp_CopyBlock_Tiled_4copy(input, output, width, height, 32);
            }
        }

        [TestCase(800, 600, repeatedCount)]
        [TestCase(1024, 1920, repeatedCount)]
        [TestCase(2000, 4000, repeatedCount)]
        public void Test_Rotate90_3bpp_Tiled64(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for (int i = 0; i < repeatedCount; i++)
            {
                RotationUtils.Rotate90ClockwiseRgb24_Tiled(input, output, width, height,64);
            }
        }

        [TestCase(800, 600, repeatedCount)]
        [TestCase(1024, 1920, repeatedCount)]
        [TestCase(2000, 4000, repeatedCount)]
        public void Test_Rotate90_3bpp_Tiled32(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for (int i = 0; i < repeatedCount; i++)
            {
                RotationUtils.Rotate90ClockwiseRgb24_Tiled(input, output, width, height,32);
            }
        }

        [TestCase(800, 600, repeatedCount)]
        [TestCase(1024, 1920, repeatedCount)]
        [TestCase(2000, 4000, repeatedCount)]
        public void Test_Rotate90_3bpp_Tiled16(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for (int i = 0; i < repeatedCount; i++)
            {
                RotationUtils.Rotate90ClockwiseRgb24_Tiled(input, output, width, height, 16);
            }
        }

        [TestCase(800, 600, repeatedCount)]
        [TestCase(1024, 1920, repeatedCount)]
        [TestCase(2000, 4000, repeatedCount)]
        public void Test_Rotate90_3bpp_Tiled8(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for (int i = 0; i < repeatedCount; i++)
            {
                RotationUtils.Rotate90ClockwiseRgb24_Tiled(input, output, width, height, 8);
            }
        }

        [TestCase(800, 600, repeatedCount)]
        [TestCase(1024, 1920, repeatedCount)]
        [TestCase(2000, 4000, repeatedCount)]
        public void Test_Rotate90_3bpp_TwoPassTransposeThenFlip(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for (int i = 0; i < repeatedCount; i++)
            {
                RotationUtils.Rotate90ClockwiseRgb24_TwoPassTransposeThenFlip(input, output, width, height);
            }
        }    
    }
}
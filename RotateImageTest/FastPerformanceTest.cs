using RotateImage;

namespace RotateImageTest
{
    public class FastPerformanceTest
    {
        const int repeatedCount = 50;

        [TestCase(800,600, repeatedCount)]
        [TestCase(1024,1920, repeatedCount)]
        [TestCase(2000,4000, repeatedCount)]
        public void Test_Rotate90ClockwiseRgb24_CopyBlock_MinTemps(int width, int height, int repeatedCount)
        {
            var input = new byte[width * height * 3];
            var output = new byte[input.Length];
            Random.Shared.NextBytes(input);
            for(int i = 0; i < repeatedCount; i++) {
                RotationUtils.Rotate90ClockwiseRgb24_CopyBlock_MinTemps(input, output, width, height);
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
    }
}
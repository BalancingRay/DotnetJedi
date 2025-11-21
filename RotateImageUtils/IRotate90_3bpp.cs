namespace Algorithms.RotateImage;

public interface IRotate90_3bpp
{
    void Rotate90Clockwise(ArraySegment<byte> data,
            byte[] destination,
            int width,
            int height);
}

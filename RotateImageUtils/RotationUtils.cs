using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RotateImage
{
    public static class RotationUtils
    {
        public static void RotateToBpp3_AsSpan(
    ArraySegment<byte> data, byte[] destination, int width, int height)
        {
            ReadOnlySpan<byte> src = data.AsSpan();
            Span<byte> dst = destination;

            int srcStride = width * 3;   // bytes per source row
            int dstRowBytes = height * 3; // bytes per destination row (newWidth * BPP)

            for (int x = 0; x < width; x++)
            {
                int xBytes = x * 3;
                int dstRowStart = x * dstRowBytes;
                int dstPos = dstRowStart + (height - 1) * 3; // write contiguous backward
                int srcPos = xBytes;

                // Copy pixel triples, avoiding Buffer.BlockCopy per pixel
                for (int y = 0; y < height; y++)
                {
                    dst[dstPos + 0] = src[srcPos + 0];
                    dst[dstPos + 1] = src[srcPos + 1];
                    dst[dstPos + 2] = src[srcPos + 2];

                    srcPos += srcStride; // down one row in source
                    dstPos -= 3;         // left one pixel in destination row
                }
            }
        }

        public static void RotateToBpp3(
ArraySegment<byte> data, byte[] destination, int width, int height)
        {
            var src = data.Array;
            var dst = destination;

            int srcStride = width * 3;   // bytes per source row
            int dstRowBytes = height * 3; // bytes per destination row (newWidth * BPP)

            for (int x = 0; x < width; x++)
            {
                int xBytes = x * 3;
                int dstRowStart = x * dstRowBytes;
                int dstPos = dstRowStart + (height - 1) * 3; // write contiguous backward
                int srcPos = xBytes + data.Offset;

                // Copy pixel triples, avoiding Buffer.BlockCopy per pixel
                for (int y = 0; y < height; y++)
                {
                    dst[dstPos + 0] = src[srcPos + 0];
                    dst[dstPos + 1] = src[srcPos + 1];
                    dst[dstPos + 2] = src[srcPos + 2];

                    srcPos += srcStride; // down one row in source
                    dstPos -= 3;         // left one pixel in destination row
                }
            }
        }

        // Variant B: Pointer increment / decrement (recommended: fewer temps than original,
        // less arithmetic than recomputing every time).
        public static unsafe byte[] Rotate90ClockwiseRgb24_CopyBlock_MinTemps(
            ArraySegment<byte> data,
            byte[] destination,
            int width,
            int height)
        {
            if (data.Array is null) throw new ArgumentNullException(nameof(data));
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            const int Bpp = 3;
            if (width < 0 || height < 0) throw new ArgumentOutOfRangeException();
            int required = checked(width * height * Bpp);
            if (data.Count < required) throw new ArgumentException("Source segment too small.", nameof(data));
            if (destination.Length < required) throw new ArgumentException("Destination too small.", nameof(destination));

            int srcStride = width * Bpp;
            int dstRowBytes = height * Bpp;

            fixed (byte* srcBase = &data.Array[data.Offset])
            fixed (byte* dstBase = &destination[0])
            {
                for (int x = 0; x < width; x++)
                {
                    // Start pointers for this column/row transform.
                    byte* srcPtr = srcBase + x * Bpp;                     // (0,x)
                    byte* dstPtr = dstBase + x * dstRowBytes + (height - 1) * Bpp; // rightmost pixel of destination row

                    for (int y = 0; y < height; y++)
                    {
                        Unsafe.CopyBlockUnaligned(dstPtr, srcPtr, (uint)Bpp);

                        // Advance down one source row (same column)
                        srcPtr += srcStride;

                        // Move left one pixel in destination row
                        dstPtr -= Bpp;
                    }
                }
            }
            return destination;
        }
    }
}

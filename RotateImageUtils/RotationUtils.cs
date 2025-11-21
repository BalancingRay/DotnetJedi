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
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe byte[] Rotate_3bpp_CopyBlock_MinTemps(
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

        // Cache-friendly tiled 90° clockwise RGB24 rotation.
        // Keeps working set hot by operating on tiles; significantly reduces cache misses
        // for large images versus naive per-column loops.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Rotate90ClockwiseRgb24_Tiled(
            ArraySegment<byte> data,
            byte[] destination,
            int width,
            int height,
            int tileSize = 64)
        {
            if (data.Array is null) throw new ArgumentNullException(nameof(data));
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            const int Bpp = 3;
            if (width < 0 || height < 0) throw new ArgumentOutOfRangeException();
            int required = checked(width * height * Bpp);
            if (data.Count < required) throw new ArgumentException("Source segment too small.", nameof(data));
            if (destination.Length < required) throw new ArgumentException("Destination too small.", nameof(destination));
            if (tileSize <= 0) tileSize = 64;

            var src = data.Array;
            int srcOffset = data.Offset;

            int srcStride = width * Bpp;
            int dstRowBytes = height * Bpp;

            for (int ty = 0; ty < height; ty += tileSize)
            {
                int tileH = Math.Min(tileSize, height - ty);
                for (int tx = 0; tx < width; tx += tileSize)
                {
                    int tileW = Math.Min(tileSize, width - tx);

                    // Process a tile of size [tileH x tileW]
                    for (int x = 0; x < tileW; x++)
                    {
                        int xGlobal = tx + x;

                        // dst starting at row index (height - 1 - ty) for this tile
                        int dstPos = xGlobal * dstRowBytes + (height - 1 - ty) * Bpp;

                        // src starting at (ty, xGlobal)
                        int srcPos = srcOffset + ty * srcStride + xGlobal * Bpp;

                        for (int y = 0; y < tileH; y++)
                        {
                            // Copy 3-byte pixel
                            destination[dstPos + 0] = src[srcPos + 0];
                            destination[dstPos + 1] = src[srcPos + 1];
                            destination[dstPos + 2] = src[srcPos + 2];

                            srcPos += srcStride; // move down one row in source
                            dstPos -= Bpp;       // move left one pixel in destination row
                        }
                    }
                }
            }
        }

        // Two-pass approach: transpose (with tiles) then horizontal flip per row.
        // This can perform very well due to contiguous row operations in the second pass.
        // Provide an optional scratch buffer to avoid per-call allocation.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Rotate90ClockwiseRgb24_TwoPassTransposeThenFlip(
            ArraySegment<byte> data,
            byte[] destination,
            int width,
            int height,
            byte[]? scratch = null,
            int tileSize = 64)
        {
            if (data.Array is null) throw new ArgumentNullException(nameof(data));
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            const int Bpp = 3;
            if (width < 0 || height < 0) throw new ArgumentOutOfRangeException();
            int pixels = checked(width * height);
            int bytes = checked(pixels * Bpp);
            if (data.Count < bytes) throw new ArgumentException("Source segment too small.", nameof(data));
            if (destination.Length < bytes) throw new ArgumentException("Destination too small.", nameof(destination));
            if (tileSize <= 0) tileSize = 64;

            // tmp image dimensions are [height x width] in RGB24
            byte[] tmp = scratch ?? new byte[bytes];

            TransposeRgb24_Tiled(data, tmp, width, height, tileSize);

            // Horizontal flip rows from tmp into destination.
            int newWidth = height;     // after transpose, width' = height
            int newHeight = width;     // after transpose, height' = width
            int rowBytes = newWidth * Bpp;

            for (int y = 0; y < newHeight; y++)
            {
                int rowStartDst = y * rowBytes;
                int rowStartSrc = y * rowBytes;

                // Copy pixels from right to left in tmp to left to right in destination
                int leftDst = rowStartDst;
                int rightSrc = rowStartSrc + (newWidth - 1) * Bpp;

                for (int x = 0; x < newWidth; x++)
                {
                    // Copy 3-byte pixel
                    destination[leftDst + 0] = tmp[rightSrc + 0];
                    destination[leftDst + 1] = tmp[rightSrc + 1];
                    destination[leftDst + 2] = tmp[rightSrc + 2];

                    leftDst += Bpp;
                    rightSrc -= Bpp;
                }
            }
        }

        //// Convenience overloads that accept byte[] input
        //private static void Rotate90_3bpp_Tiled(
        //    byte[] data, byte[] destination, int width, int height, int tileSize = 64)
        //    => Rotate90ClockwiseRgb24_Tiled(new ArraySegment<byte>(data), destination, width, height, tileSize);

        //private static void Rotate90_3bpp_TwoPassTransposeThenFlip(
        //    byte[] data, byte[] destination, int width, int height, byte[]? scratch = null, int tileSize = 64)
        //    => Rotate90ClockwiseRgb24_TwoPassTransposeThenFlip(new ArraySegment<byte>(data), destination, width, height, scratch, tileSize);

        // Internal: tiled transpose for RGB24: tmp[x, y] = src[y, x]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void TransposeRgb24_Tiled(
            ArraySegment<byte> data,
            byte[] tmp,
            int width,
            int height,
            int tileSize)
        {
            const int Bpp = 3;
            var src = data.Array!;
            int srcOffset = data.Offset;

            // tmp has dimensions [height x width]
            int tmpStride = height * Bpp;   // new width (height) * Bpp
            int srcStride = width * Bpp;

            for (int ty = 0; ty < height; ty += tileSize)
            {
                int tileH = Math.Min(tileSize, height - ty);
                for (int tx = 0; tx < width; tx += tileSize)
                {
                    int tileW = Math.Min(tileSize, width - tx);

                    for (int y = 0; y < tileH; y++)
                    {
                        int yGlobal = ty + y;
                        int srcRow = srcOffset + yGlobal * srcStride;
                        for (int x = 0; x < tileW; x++)
                        {
                            int xGlobal = tx + x;

                            int srcPos = srcRow + xGlobal * Bpp;
                            int tmpPos = xGlobal * tmpStride + yGlobal * Bpp;

                            tmp[tmpPos + 0] = src[srcPos + 0];
                            tmp[tmpPos + 1] = src[srcPos + 1];
                            tmp[tmpPos + 2] = src[srcPos + 2];
                        }
                    }
                }
            }
        }
    }
}

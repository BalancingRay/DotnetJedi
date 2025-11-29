using RotateImageUtils;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace RotateImage
{
    public static class RotationUtils
    {
        // SSSE3 shuffle masks: picks 3 bytes from each 32-bit lane in reverse lane order.
        // PackMaskAvx2 is separate so mask tuning can be done per microarchitecture if profiling shows benefits.
        private static readonly Vector128<byte> PackMaskSse41 = Vector128.Create(
            (byte)12, 13, 14,   // p3
            8, 9, 10,           // p2
            4, 5, 6,            // p1
            0, 1, 2,            // p0
            0x80, 0x80, 0x80, 0x80 // ignore high 4 bytes
        );

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void RotateToBpp3_AsSpan(
    ArraySegment<byte> data, byte[] destination, int width, int height)
        {
            if (data.Array == null)
                throw new ArgumentException("Null input buffer");
            int required = checked(width * height * 3);
            if (data.Array.Length < required || destination.Length < required)
                throw new ArgumentException("Invalid buffer size");
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

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe byte[] Rotate_3bpp_Span_CopyBlock(
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
            ReadOnlySpan<byte> src = data;
            Span<byte> dst = destination;
            fixed (byte* srcBase = &src[data.Offset])
            fixed (byte* dstBase = &dst[0])
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

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void Rotate_3bpp_CopyBlock_MinTemps_Stackalloc(
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
            const int maxStackAlloc = 1024 * 32; // 32 KB max stackalloc size
            int srcStride = width * Bpp;     // bytes per source row
            int dstRowBytes = height * Bpp;  // bytes per destination row (new width = height)
            if (dstRowBytes > maxStackAlloc)
                throw new ArgumentException("heigth parameter is too large for this algorithm", nameof(data));
            ReadOnlySpan<byte> src = data;
            Span<byte> dst = destination;
            fixed (byte* srcBase = &src[data.Offset])
            fixed (byte* dstBase = &dst[0])
            {
                byte* basePtr = stackalloc byte[dstRowBytes];

                for (int x = 0; x < width; x++)
                {
                    // Initialize temp buffer: fill destination row for column x
                    // dst row address for this column = dstBase + x * dstRowBytes
                    // mapping: dst[x, height - 1 - y] = src[y, x]
                    // temp buffer holds the entire destination row for this x, written contiguously.
                    byte* tempPtr = basePtr + (height - 1) * Bpp;
                    byte* srcPtr = srcBase + x * Bpp;
                    for (int y = 0; y < height; y++)
                    {
                        // copy 3-byte pixel into its reversed position in temp
                        Unsafe.CopyBlockUnaligned(tempPtr, srcPtr, Bpp);
                        srcPtr += srcStride;
                        tempPtr -= Bpp;
                    }

                    // Bulk copy the prepared destination row
                    byte* dstRowPtr = dstBase + x * dstRowBytes;
                    Unsafe.CopyBlockUnaligned(dstRowPtr, basePtr, (uint)dstRowBytes);
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void Rotate_3bpp_CopyBlock_Tiled(
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

            int srcStride = width * Bpp;
            int dstRowBytes = height * Bpp;

            ReadOnlySpan<byte> src = data;
            Span<byte> dst = destination;
            fixed (byte* srcBase = &src[data.Offset])
            fixed (byte* dstBase = &dst[0])
            {
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

                            // dst starting at row index (height - 1 - ty) for this tile, column xGlobal
                            byte* dstPtr = dstBase + xGlobal * dstRowBytes + (height - 1 - ty) * Bpp;

                            // src starting at (ty, xGlobal)
                            byte* srcPtr = srcBase + ty * srcStride + xGlobal * Bpp;

                            for (int y = 0; y < tileH; y++)
                            {
                                Unsafe.CopyBlockUnaligned(dstPtr, srcPtr, Bpp);

                                // move down one row in source (same column)
                                srcPtr += srcStride;

                                // move left one pixel in destination row
                                dstPtr -= Bpp;
                            }
                        }
                    }
                }
            }
        }


        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void Rotate_3bpp_CopyBlock_Tiled_vector256(
             ArraySegment<byte> data,
             byte[] destination,
             int width,
             int height)
        {
            const int tileSize = 3;
            if (data.Array is null) throw new ArgumentNullException(nameof(data));
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            const int Bpp = 3;
            if (width < 0 || height < 0) throw new ArgumentOutOfRangeException();
            int required = checked(width * height * Bpp);
            if (data.Count < required) throw new ArgumentException("Source segment too small.", nameof(data));
            if (destination.Length < required) throw new ArgumentException("Destination too small.", nameof(destination));
            if (!Sse2.IsSupported || !Ssse3.IsSupported)
                throw new PlatformNotSupportedException("SSSE3 required for shuffle-based tile transform");

            int srcStride = width * Bpp;
            int dstRowBytes = height * Bpp;

            ReadOnlySpan<byte> src = data;
            Span<byte> dst = destination;

            Vector256<byte> transform = Vector256.Create(
                (byte)18, 19, 20, 9, 10, 11, 0, 1, 2,
                      21, 22, 23, 12, 13, 14, 3, 4, 5,
                      24, 25, 26, 15, 16, 17, 6, 7, 8,
                      127, 127, 127, 127, 127);

            fixed (byte* srcBase = &src[data.Offset])
            fixed (byte* dstBase = &dst[0])
            {
                byte* tile = stackalloc byte[32];
                for (int tileY = 0; tileY < height; tileY += tileSize)
                {
                    int tileH = Math.Min(tileSize, height - tileY);
                    for (int tileX = 0; tileX < width; tileX += tileSize)
                    {
                        int tileW = Math.Min(tileSize, width - tileX);

                        if (tileW == tileSize && tileH == tileSize)
                        {
                            // Stack buffer for 32 bytes tile payload (fits 27 bytes + pad)


                            // 1) Copy selected tile data to the buffer as 3 operations by 3 bytes per pixel
                            // Tile is laid out row-major: rows ty..ty+2, cols tx..tx+2
                            byte* tPtr = tile;
                            for (int i = 0; i < tileSize; i++)
                            {
                                int yGlobal = tileY + i;
                                byte* srcRow = srcBase + yGlobal * srcStride + tileX * Bpp;
                                // copy 3 pixels (9 bytes) of this row
                                Unsafe.CopyBlockUnaligned(tPtr, srcRow, 9);
                                tPtr += 9;
                            }

                            // 2) Use predefined transformation vector to rotate tile 90° CW
                            Vector256<byte> tileVector = Avx.LoadDquVector256(tile);         // bytes 0..31
                            Vector256<byte> rot = Vector256.Shuffle(tileVector, transform);


                            // 4) Copy tile to destination as 3 operations by 3 bytes (per destination row)
                            // Destination coordinates for 90° CW:
                            // dst has dimensions [height x width]; a 3x3 source tile at (ty,tx) maps to:
                            // columns xGlobal = tx..tx+2 become rows in dst; we write rows for x=tx..tx+2
                            // For a column xGlobal, its destination row starts at:
                            // dstRowStart = xGlobal * dstRowBytes, and within the row y maps to (height-1 - y)
                            // Here, for each xGlobal in tx..tx+2, we copy a contiguous run of 3 pixels from rotated tile.
                            // The rotated tile is in row-major order already.
                            rot.Store(tile);
                            byte* srcRotRow = tile;
                            //byte* tilePtr = rot;
                            for (int i = 0; i < tileSize; i++)
                            {
                                // destination row base for this column
                                int destinationYshift = (tileX + i) * dstRowBytes;
                                int destinationXshift = dstRowBytes - (tileY + tileSize) * Bpp;
                                byte* d = dstBase + (destinationYshift + destinationXshift);
                                // copy 3 pixels (9 bytes) from rotated tile row
                                Unsafe.CopyBlockUnaligned(d, srcRotRow, 9);
                                srcRotRow += 9;
                            }
                        }
                        else
                        {
                            // Fallback for partial tiles at borders: use the existing pointer copy (correctness-first)
                            for (int x = 0; x < tileW; x++)
                            {
                                int xGlobal = tileX + x;
                                byte* dstPtr = dstBase + xGlobal * dstRowBytes + (height - 1 - tileY) * Bpp;
                                byte* srcPtr = srcBase + tileY * srcStride + xGlobal * Bpp;

                                for (int y = 0; y < tileH; y++)
                                {
                                    Unsafe.CopyBlockUnaligned(dstPtr, srcPtr, Bpp);
                                    srcPtr += srcStride;
                                    dstPtr -= Bpp;
                                }
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void Rotate_3bpp_CopyBlock_Tiled_4copy(
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

            int srcStride = width * Bpp;
            int dstRowBytes = height * Bpp;

            ReadOnlySpan<byte> src = data;
            Span<byte> dst = destination;
            fixed (byte* srcBase = &src[data.Offset])
            fixed (byte* dstBase = &dst[0])
            {
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

                            // dst starting at row index (height - 1 - ty) for this tile, column xGlobal
                            byte* dstPtr = dstBase + xGlobal * dstRowBytes + (height - 1 - ty) * Bpp;

                            // src starting at (ty, xGlobal)
                            byte* srcPtr = srcBase + ty * srcStride + xGlobal * Bpp;

                            int y = 0;

                            // Unrolled loop: copy 4 vertical pixels per iteration
                            // Each pixel copy writes 3 bytes contiguous in destination row and then moves left.
                            for (; y <= tileH - 4; y += 4)
                            {
                                // Pixel 0
                                Unsafe.CopyBlockUnaligned(dstPtr, srcPtr, Bpp);
                                srcPtr += srcStride;
                                dstPtr -= Bpp;

                                // Pixel 1
                                Unsafe.CopyBlockUnaligned(dstPtr, srcPtr, Bpp);
                                srcPtr += srcStride;
                                dstPtr -= Bpp;

                                // Pixel 2
                                Unsafe.CopyBlockUnaligned(dstPtr, srcPtr, Bpp);
                                srcPtr += srcStride;
                                dstPtr -= Bpp;

                                // Pixel 3
                                Unsafe.CopyBlockUnaligned(dstPtr, srcPtr, Bpp);
                                srcPtr += srcStride;
                                dstPtr -= Bpp;
                            }

                            // Tail (0–3 remaining)
                            for (; y < tileH; y++)
                            {
                                Unsafe.CopyBlockUnaligned(dstPtr, srcPtr, Bpp);
                                srcPtr += srcStride;
                                dstPtr -= Bpp;
                            }
                        }
                    }
                }
            }
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
            int bytes = checked(width * height * Bpp);
            if (data.Array == null)
                throw new ArgumentException("Null input buffer");
            if (data.Array.Length < bytes || tmp.Length < bytes)
                throw new ArgumentException("Invalid buffer size");
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

        /// <summary>
        /// Rotates a 24-bit (3 BPP) image 90 degrees clockwise using SSE4.1 + SSSE3 + AVX2.
        /// Optimized for minimal allocations and maximal throughput.  
        /// Processes 4 pixels per iteration using 16-byte SIMD loads and byte-shuffle.
        /// Scalar fallback handles remaining rows.  
        /// Source and destination buffers must match width * height * 3.
        /// </summary>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void RotateToBpp3_Unsafe_SSE41(
            ReadOnlySpan<byte> src,
            Span<byte> dst,
            int width,
            int height)
        {
            // Avx2.IsSupported == true in practice on x64 desktops means: there is SSE2(required), there is SSSE3, there is SSE4.1.
            // On Intel/AMD x64, if there is AVX2, you actually won't find a CPU without SSSE3/SSE4.1. For your target (Windows desktop/server, .NET 10) this assumption is more than enough.
            if (!Avx2.IsSupported)
                throw new PlatformNotSupportedException("AVX2 + SSSE3 + SSE4.1 required");

            int bytes = checked(width * height * 3);
            if (src.Length < bytes || dst.Length < bytes)
                throw new ArgumentException("Invalid buffer size");

            int srcStride = width * 3;
            int dstStride = height * 3;

            // SSSE3 mask: p3 | p2 | p1 | p0 (3 bytes from each 32-bit lane)
            Vector128<byte> packMask = Vector128.Create(
                (byte)12, 13, 14,
                8, 9, 10,
                4, 5, 6,
                0, 1, 2,
                0x80, 0x80, 0x80, 0x80
            );

            fixed (byte* pSrcFixed = src)
            fixed (byte* pDstFixed = dst)
            {
                byte* pSrc = pSrcFixed;
                byte* pDst = pDstFixed;

                for (int x = 0; x < width; x++)
                {
                    byte* srcCol = pSrc + x * 3;
                    byte* dstCol = pDst + x * dstStride + (height - 1) * 3;

                    int y = 0;
                    byte* srcPtr = srcCol;
                    byte* dstBlock = dstCol - 9; // position for pixel (y+3)

                    for (; y <= height - 4; y += 4, srcPtr += srcStride * 4, dstBlock -= 12)
                    {
                        byte* p0 = srcPtr;
                        byte* p1 = p0 + srcStride;
                        byte* p2 = p1 + srcStride;
                        byte* p3 = p2 + srcStride;

                        uint v0 = (uint)(p0[0] | (p0[1] << 8) | (p0[2] << 16));
                        uint v1 = (uint)(p1[0] | (p1[1] << 8) | (p1[2] << 16));
                        uint v2 = (uint)(p2[0] | (p2[1] << 8) | (p2[2] << 16));
                        uint v3 = (uint)(p3[0] | (p3[1] << 8) | (p3[2] << 16));

                        Vector128<uint> packed = Vector128.Create(v0, v1, v2, v3);
                        Vector128<byte> collapsed = Ssse3.Shuffle(packed.AsByte(), packMask);

                        ((ulong*)dstBlock)[0] = collapsed.AsUInt64().GetElement(0);
                        ((uint*)(dstBlock + 8))[0] = collapsed.AsUInt32().GetElement(2);
                    }

                    for (; y < height; y++)
                    {
                        byte* s = srcCol + y * srcStride;
                        byte* d = dstCol - y * 3;
                        d[0] = s[0]; d[1] = s[1]; d[2] = s[2];
                    }
                }
            }
        }

        /// <summary>
        /// Rotates a 24-bit (3 BPP) image 90 degrees clockwise using unsafe pointers
        /// combined with multi-threaded processing via Parallel.For.  
        /// No SIMD is used; each thread processes a horizontal row independently.  
        /// Suitable for CPUs without SSSE3/SSE4.1 support.  
        /// Source and destination buffers must match width * height * 3.
        /// </summary>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void RotateToBpp3_Unsafe_Parallel(
            ReadOnlySpan<byte> src,
            Span<byte> dst,
            int width,
            int height,
            int maxThreads = 6)
        {
            int bytes = checked(width * height * 3);
            if (src.Length < bytes || dst.Length < bytes)
                throw new ArgumentException("Invalid buffer size");

            int srcStride = width * 3;
            int dstStride = height * 3; // dstWidth = height

            ParallelOptions po = new()
            {
                MaxDegreeOfParallelism = maxThreads
            };

            fixed (byte* pSrcFixed = src)
            fixed (byte* pDstFixed = dst)
            {
                byte* srcBase = pSrcFixed;
                byte* dstBase = pDstFixed;

                // now they are NOT captured variables → allowed by the compiler
                Parallel.For(0, height, po, y =>
                {
                    byte* srcRow = srcBase + (nint)(y * srcStride);

                    // dstX = height - 1 - y
                    int dstX = height - 1 - y;
                    byte* dstColBase = dstBase + (nint)(dstX * 3);

                    for (int x = 0; x < width; x++)
                    {
                        byte* s = srcRow + (nint)(x * 3);
                        byte* d = dstColBase + (nint)(x * dstStride);

                        // copy 3 bytes
                        d[0] = s[0];
                        d[1] = s[1];
                        d[2] = s[2];
                    }
                });
            }
        }

        /// <summary>
        /// Rotates a 24-bit (3 BPP) image 90 degrees clockwise using SSE4.1 + SSSE3 + AVX2  
        /// inside a multi-threaded Parallel.For loop.  
        /// Loads 16 bytes, reverses with SSSE3 shuffle mask, then extracts three 32-bit
        /// values containing the rotated 12-byte block.  
        /// Provides maximum throughput on modern x64 CPUs with full SIMD support.
        /// Source and destination buffers must match width * height * 3.
        /// </summary>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void RotateToBpp3_Unsafe_Parallel_SSE41(
            ReadOnlySpan<byte> src,
            Span<byte> dst,
            int width,
            int height,
            int maxThreads = 6)
        {
            // Avx2.IsSupported == true in practice on x64 desktops means: there is SSE2(required), there is SSSE3, there is SSE4.1.
            // On Intel/AMD x64, if there is AVX2, you actually won't find a CPU without SSSE3/SSE4.1. For your target (Windows desktop/server, .NET 10) this assumption is more than enough.
            if (!Avx2.IsSupported)
                throw new PlatformNotSupportedException("AVX2 + SSSE3 + SSE4.1 required");

            int bytes = checked(width * height * 3);
            if (src.Length < bytes || dst.Length < bytes)
                throw new ArgumentException("Invalid buffer size");

            int srcStride = width * 3;
            int dstStride = height * 3;

            ParallelOptions po = new()
            {
                MaxDegreeOfParallelism = maxThreads
            };

            fixed (byte* pSrcFixed = src)
            fixed (byte* pDstFixed = dst)
            {
                byte* pSrc = pSrcFixed;
                byte* pDst = pDstFixed;

                Parallel.For(0, width, po, x =>
                {
                    byte* srcCol = pSrc + x * 3;
                    byte* dstCol = pDst + x * dstStride + (height - 1) * 3;

                    int y = 0;
                    byte* srcPtr = srcCol;
                    byte* dstBlock = dstCol - 9; // first block writes pixels (y..y+3) -> start at y+3
                    for (; y <= height - 4; y += 4, srcPtr += srcStride * 4, dstBlock -= 12)
                    {
                        byte* p0 = srcPtr;
                        byte* p1 = p0 + srcStride;
                        byte* p2 = p1 + srcStride;
                        byte* p3 = p2 + srcStride;

                        uint v0 = (uint)(p0[0] | (p0[1] << 8) | (p0[2] << 16));
                        uint v1 = (uint)(p1[0] | (p1[1] << 8) | (p1[2] << 16));
                        uint v2 = (uint)(p2[0] | (p2[1] << 8) | (p2[2] << 16));
                        uint v3 = (uint)(p3[0] | (p3[1] << 8) | (p3[2] << 16));

                        Vector128<uint> packed = Vector128.Create(v0, v1, v2, v3);
                        Vector128<byte> collapsed = Ssse3.Shuffle(packed.AsByte(), PackMaskSse41);

                        ((ulong*)dstBlock)[0] = collapsed.AsUInt64().GetElement(0);
                        ((uint*)(dstBlock + 8))[0] = collapsed.AsUInt32().GetElement(2);
                    }

                    for (; y < height; y++)
                    {
                        byte* s = srcCol + y * srcStride;
                        byte* d = dstCol - y * 3;

                        d[0] = s[0];
                        d[1] = s[1];
                        d[2] = s[2];
                    }
                });
            }
        }

        /// <summary>
        /// Rotates a 24-bit (3 BPP) image 90 degrees clockwise using SSE4.1 + SSSE3 + AVX2  
        /// inside a multi-threaded loop with minimal allocations  
        /// Loads 16 bytes, reverses with SSSE3 shuffle mask, then extracts three 32-bit
        /// values containing the rotated 12-byte block.  
        /// Provides maximum throughput on modern x64 CPUs with full SIMD support.
        /// Source and destination buffers must match width * height * 3.
        /// Designed for high-throughput image pipelines (e.g., GigE Vision) with minimal allocations
        /// and maximum performance. The method pins the source and destination buffers and performs a
        /// single native call, where the actual rotation and parallelization are executed.
        /// </summary>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void RotateToBpp3_Unsafe_Parallel_SSE41_Native(
           ReadOnlySpan<byte> src,
           Span<byte> dst,
           int width,
           int height,
           int maxThreads = 6)
        {
            // Avx2.IsSupported == true in practice on x64 desktops means: there is SSE2(required), there is SSSE3, there is SSE4.1.
            // On Intel/AMD x64, if there is AVX2, you actually won't find a CPU without SSSE3/SSE4.1. For your target (Windows desktop/server, .NET 10) this assumption is more than enough.
            if (!Avx2.IsSupported)
                throw new PlatformNotSupportedException("AVX2 + SSSE3 + SSE4.1 required");

            int bytes = checked(width * height * 3);
            if (src.Length < bytes || dst.Length < bytes)
                throw new ArgumentException("Invalid buffer size (RGB24).");

            fixed (byte* pSrc = src)
            fixed (byte* pDst = dst)
            {
                NativeRotate.RotateRgb24_90cw_sse41(
                    pSrc,
                    pDst,
                    width,
                    height,
                    maxThreads);
            }
        }
    }
}

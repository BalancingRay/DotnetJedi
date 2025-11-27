using RotateImageUtils;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;

namespace RotateImage
{
    public static class RotationUtils
    {
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
            if(data.Array == null)
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
            int height,
            int maxThreads = 6)
        {
            if (!Avx2.IsSupported || !Ssse3.IsSupported || !Sse41.IsSupported)
                throw new PlatformNotSupportedException("AVX2 + SSSE3 + SSE4.1 required");

            int bytes = checked(width * height * 3);
            if (src.Length < bytes || dst.Length < bytes)
                throw new ArgumentException("Invalid buffer size");

            int srcStride = width * 3;
            int dstStride = height * 3;

            fixed (byte* pSrcFixed = src)
            fixed (byte* pDstFixed = dst)
            {
                byte* pSrc = pSrcFixed;
                byte* pDst = pDstFixed;

                Vector128<byte> reverseMask = Vector128.Create(
                    (byte)15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0
                );

                for (var x = 0; x < width; x++)
                {
                    byte* srcCol = pSrc + x * 3;
                    byte* dstCol = pDst + x * dstStride + (height - 1) * 3;

                    int y = 0;
                    for (; y <= height - 4; y += 4)
                    {
                        byte* pSrc4 = srcCol + y * srcStride;
                        byte* pDst4 = dstCol - y * 3;

                        Vector128<byte> v = Sse2.LoadVector128(pSrc4);
                        Vector128<byte> r = Ssse3.Shuffle(v, reverseMask);

                        // r: the required 12 bytes are in the elements uint[1], uint[2], uint[3]
                        Vector128<uint> r32 = r.AsUInt32();

                        uint v1 = r32.GetElement(1);
                        uint v2 = r32.GetElement(2);
                        uint v3 = r32.GetElement(3);

                        uint* d32 = (uint*)(pDst4 - 12);
                        d32[0] = v1;
                        d32[1] = v2;
                        d32[2] = v3;
                    }

                    for (; y < height; y++)
                    {
                        byte* s = srcCol + y * srcStride;
                        byte* d = dstCol - y * 3;

                        d[0] = s[0];
                        d[1] = s[1];
                        d[2] = s[2];
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
                // создаём unmanaged указатели ВНЕ лямбды
                byte* srcBase = pSrcFixed;
                byte* dstBase = pDstFixed;

                // теперь они НЕ являются captured variables → разрешено компилятором
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
        /// Rotates a 24-bit (3 BPP) image 90 degrees clockwise using SSSE3 acceleration
        /// inside a Parallel.For loop.  
        /// Performs 16-byte SIMD loads and byte-reversal via SSSE3 shuffle mask,  
        /// writing 12 output bytes per 4-pixel block.  
        /// Uses stackalloc for temporary SIMD buffer to avoid heap allocations.
        /// Source and destination buffers must match width * height * 3.
        /// </summary>
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void RotateToBpp3_Unsafe_Parallel_SSSE3(
            ReadOnlySpan<byte> src,
            Span<byte> dst,
            int width,
            int height,
            int maxThreads = 6)
        {
            if (!Sse2.IsSupported || !Ssse3.IsSupported)
                throw new PlatformNotSupportedException("Sse2 + SSSE3 required");

            int bytes = checked(width * height * 3);
            if (src.Length < bytes || dst.Length < bytes)
                throw new ArgumentException("Invalid buffer size");

            int srcStride = width * 3;
            int dstStride = height * 3;

            var po = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxThreads
            };

            fixed (byte* pSrcFixed = src)
            fixed (byte* pDstFixed = dst)
            {
                byte* pSrc = pSrcFixed;
                byte* pDst = pDstFixed;

                Vector128<byte> reverseMask = Vector128.Create(
                    (byte)15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0
                );

                Parallel.For(0, width, po, x =>
                {
                    byte* srcCol = pSrc + x * 3;
                    byte* dstCol = pDst + x * dstStride + (height - 1) * 3;

                    byte* tmp = stackalloc byte[16];

                    int y = 0;
                    for (; y <= height - 4; y += 4)
                    {
                        byte* pSrc4 = srcCol + y * srcStride;
                        byte* pDst4 = dstCol - y * 3;

                        var v = Sse2.LoadVector128(pSrc4);
                        var r = Ssse3.Shuffle(v, reverseMask);

                        Sse2.Store(tmp, r);

                        // копируем 12 байт: 8 + 4
                        //*(ulong*)(pDst4 - 12) = *(ulong*)(tmp + 4);
                        //*(int*)(pDst4 - 4) = *(int*)(tmp + 12);

                        Unsafe.CopyBlockUnaligned(pDst4 - 12, tmp + 4, 12);
                    }

                    for (; y < height; y++)
                    {
                        byte* s = srcCol + y * srcStride;
                        byte* d = dstCol - y * 3;

                        Unsafe.CopyBlockUnaligned(d, s, 3);
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
            if (!Avx2.IsSupported || !Ssse3.IsSupported || !Sse41.IsSupported)
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

                Vector128<byte> reverseMask = Vector128.Create(
                    (byte)15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0
                );

                Parallel.For(0, width, po, x =>
                {
                    byte* srcCol = pSrc + x * 3;
                    byte* dstCol = pDst + x * dstStride + (height - 1) * 3;

                    int y = 0;
                    for (; y <= height - 4; y += 4)
                    {
                        byte* pSrc4 = srcCol + y * srcStride;
                        byte* pDst4 = dstCol - y * 3;

                        Vector128<byte> v = Sse2.LoadVector128(pSrc4);
                        Vector128<byte> r = Ssse3.Shuffle(v, reverseMask);

                        // r: the required 12 bytes are in the elements uint[1], uint[2], uint[3]
                        Vector128<uint> r32 = r.AsUInt32();

                        uint v1 = r32.GetElement(1);
                        uint v2 = r32.GetElement(2);
                        uint v3 = r32.GetElement(3);

                        uint* d32 = (uint*)(pDst4 - 12);
                        d32[0] = v1;
                        d32[1] = v2;
                        d32[2] = v3;
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
            if (!Avx2.IsSupported || !Ssse3.IsSupported || !Sse41.IsSupported)
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

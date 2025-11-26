#include "pch.h"
#include "RgbRotateNative.h"
#include <cstdint>
#include <immintrin.h>

// Option OpenMP: В project settings: C/C++ → Language → OpenMP Support: Yes (/openmp)

extern "C" __declspec(dllexport)
void __stdcall RotateRgb24_90cw_sse41(
    const std::uint8_t* src,
    std::uint8_t* dst,
    int width,
    int height,
    int maxThreads)
{
    const int srcStride = width * 3;
    const int dstStride = height * 3;

    const __m128i reverseMask = _mm_setr_epi8(
        15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0
    );

#pragma omp parallel for num_threads(maxThreads)
    for (int x = 0; x < width; ++x)
    {
        const std::uint8_t* srcCol = src + x * 3;
        std::uint8_t* dstCol = dst + x * dstStride + (height - 1) * 3;

        int y = 0;

        // основной SIMD-цикл: обрабатываем 4 пикселя по вертикали
        for (; y <= height - 4; y += 4)
        {
            const std::uint8_t* pSrc4 = srcCol + y * srcStride;
            std::uint8_t* pDst4 = dstCol - y * 3;

            // load 16 bytes (5 pixels of 3 bytes + tail → we need the first 12 bytes after shuffle)
            __m128i v = _mm_loadu_si128(reinterpret_cast<const __m128i*>(pSrc4));
            __m128i r = _mm_shuffle_epi8(v, reverseMask);

            // r contains the data we need in uint[1], uint[2], uint[3]
            std::uint32_t v1 = static_cast<std::uint32_t>(_mm_extract_epi32(r, 1));
            std::uint32_t v2 = static_cast<std::uint32_t>(_mm_extract_epi32(r, 2));
            std::uint32_t v3 = static_cast<std::uint32_t>(_mm_extract_epi32(r, 3));

            std::uint32_t* d32 = reinterpret_cast<std::uint32_t*>(pDst4 - 12);
            d32[0] = v1;
            d32[1] = v2;
            d32[2] = v3;
        }

        for (; y < height; ++y)
        {
            const std::uint8_t* s = srcCol + y * srcStride;
            std::uint8_t* d = dstCol - y * 3;

            d[0] = s[0];
            d[1] = s[1];
            d[2] = s[2];
        }
    }
}
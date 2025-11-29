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

    const __m128i packMask = _mm_setr_epi8(
        12, 13, 14,  // p3
        8, 9, 10,  // p2
        4, 5, 6,  // p1
        0, 1, 2,  // p0
        -1, -1, -1, -1 // ignore high bytes
    );

#pragma omp parallel for num_threads(maxThreads)
    for (int x = 0; x < width; ++x)
    {
        const std::uint8_t* srcCol = src + x * 3;
        std::uint8_t* dstCol = dst + x * dstStride + (height - 1) * 3;

        int y = 0;

        // основной SIMD-цикл: обрабатываем 4 пикселя по вертикали
        for (int y = 0; y <= height - 4; y += 4) {
            const std::uint8_t* p0 = srcCol + y * srcStride;
            const std::uint8_t* p1 = p0 + srcStride;
            const std::uint8_t* p2 = p1 + srcStride;
            const std::uint8_t* p3 = p2 + srcStride;

            std::uint32_t v0 = p0[0] | (p0[1] << 8) | (p0[2] << 16);
            std::uint32_t v1 = p1[0] | (p1[1] << 8) | (p1[2] << 16);
            std::uint32_t v2 = p2[0] | (p2[1] << 8) | (p2[2] << 16);
            std::uint32_t v3 = p3[0] | (p3[1] << 8) | (p3[2] << 16);

            __m128i lanes = _mm_setr_epi32(v0, v1, v2, v3);
            __m128i collapsed = _mm_shuffle_epi8(lanes, packMask);

            std::uint8_t* dstBlock = dstCol - (y + 3) * 3; // start at pixel y+3
            *reinterpret_cast<std::uint64_t*>(dstBlock) = _mm_cvtsi128_si64(collapsed);          // 8 байт
            *reinterpret_cast<std::uint32_t*>(dstBlock + 8) = _mm_extract_epi32(collapsed, 2);       // ещё 4 байта
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
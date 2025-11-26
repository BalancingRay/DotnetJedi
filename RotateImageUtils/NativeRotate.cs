using System.Runtime.InteropServices;

namespace RotateImageUtils
{
    internal static partial class NativeRotate
    {
        // DLL name (without .dll)
        private const string NativeLib = "x64\\RotateNative";

        [LibraryImport(NativeLib, EntryPoint = "RotateRgb24_90cw_sse41", SetLastError = false)]
        internal static unsafe partial void RotateRgb24_90cw_sse41(
            byte* src,
            byte* dst,
            int width,
            int height,
            int maxThreads);
    }
}

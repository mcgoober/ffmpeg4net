using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;

namespace SharpFFmpeg
{
    public partial class FFmpeg
    {
        /// <summary>
        /// Returns the LIBAVUTIL_VERSION_INT constant.
        /// </summary>        
        [DllImport(AVUTILS_DLL_NAME), SuppressUnmanagedCodeSecurity]
        public static extern uint avutil_version();

        public delegate int Read_PacketCallback(IntPtr opaque, IntPtr buf, int buf_size);

        public delegate int WritePacketCallback(IntPtr opaque, IntPtr buf, int buf_size);

        public delegate Int64 SeekCallback(IntPtr opaque, Int64 offset, int whence);

        public delegate UInt32 UpdateChecksumCallback(UInt32 checksum, IntPtr buf, UInt32 size);

        public enum PixelFormat
        {
            PIX_FMT_NONE = -1,
            PIX_FMT_YUV420P,   // Planar YUV 4:2:0, 12bpp, (1 Cr & Cb sample per 2x2 Y samples)
            PIX_FMT_YUYV422,   // Packed YUV 4:2:2, 16bpp, Y0 Cb Y1 Cr
            PIX_FMT_RGB24,     // Packed RGB 8:8:8, 24bpp, RGBRGB...
            PIX_FMT_BGR24,     // Packed RGB 8:8:8, 24bpp, BGRBGR...
            PIX_FMT_YUV422P,   // Planar YUV 4:2:2, 16bpp, (1 Cr & Cb sample per 2x1 Y samples)
            PIX_FMT_YUV444P,   // Planar YUV 4:4:4, 24bpp, (1 Cr & Cb sample per 1x1 Y samples)
            PIX_FMT_RGB32,     // Packed RGB 8:8:8, 32bpp, (msb)8A 8R 8G 8B(lsb), in cpu endianness
            PIX_FMT_YUV410P,   // Planar YUV 4:1:0,  9bpp, (1 Cr & Cb sample per 4x4 Y samples)
            PIX_FMT_YUV411P,   // Planar YUV 4:1:1, 12bpp, (1 Cr & Cb sample per 4x1 Y samples)
            PIX_FMT_RGB565,    // Packed RGB 5:6:5, 16bpp, (msb)   5R 6G 5B(lsb), in cpu endianness
            PIX_FMT_RGB555,    // Packed RGB 5:5:5, 16bpp, (msb)1A 5R 5G 5B(lsb), in cpu endianness most significant bit to 1
            PIX_FMT_GRAY8,     //        Y        ,  8bpp
            PIX_FMT_MONOWHITE, //        Y        ,  1bpp, 1 is white
            PIX_FMT_MONOBLACK, //        Y        ,  1bpp, 0 is black
            PIX_FMT_PAL8,      // 8 bit with PIX_FMT_RGB32 palette
            PIX_FMT_YUVJ420P,  // Planar YUV 4:2:0, 12bpp, full scale (jpeg)
            PIX_FMT_YUVJ422P,  // Planar YUV 4:2:2, 16bpp, full scale (jpeg)
            PIX_FMT_YUVJ444P,  // Planar YUV 4:4:4, 24bpp, full scale (jpeg)
            PIX_FMT_XVMC_MPEG2_MC,// XVideo Motion Acceleration via common packet passing(xvmc_render.h)
            PIX_FMT_XVMC_MPEG2_IDCT,
            PIX_FMT_UYVY422,   // Packed YUV 4:2:2, 16bpp, Cb Y0 Cr Y1
            PIX_FMT_UYYVYY411, // Packed YUV 4:1:1, 12bpp, Cb Y0 Y1 Cr Y2 Y3
            PIX_FMT_BGR32,     // Packed RGB 8:8:8, 32bpp, (msb)8A 8B 8G 8R(lsb), in cpu endianness
            PIX_FMT_BGR565,    // Packed RGB 5:6:5, 16bpp, (msb)   5B 6G 5R(lsb), in cpu endianness
            PIX_FMT_BGR555,    // Packed RGB 5:5:5, 16bpp, (msb)1A 5B 5G 5R(lsb), in cpu endianness most significant bit to 1
            PIX_FMT_BGR8,      // Packed RGB 3:3:2,  8bpp, (msb)2B 3G 3R(lsb)
            PIX_FMT_BGR4,      // Packed RGB 1:2:1,  4bpp, (msb)1B 2G 1R(lsb)
            PIX_FMT_BGR4_BYTE, // Packed RGB 1:2:1,  8bpp, (msb)1B 2G 1R(lsb)
            PIX_FMT_RGB8,      // Packed RGB 3:3:2,  8bpp, (msb)2R 3G 3B(lsb)
            PIX_FMT_RGB4,      // Packed RGB 1:2:1,  4bpp, (msb)2R 3G 3B(lsb)
            PIX_FMT_RGB4_BYTE, // Packed RGB 1:2:1,  8bpp, (msb)2R 3G 3B(lsb)
            PIX_FMT_NV12,      // Planar YUV 4:2:0, 12bpp, 1 plane for Y and 1 for UV
            PIX_FMT_NV21,      // as above, but U and V bytes are swapped

            PIX_FMT_RGB32_1,   // Packed RGB 8:8:8, 32bpp, (msb)8R 8G 8B 8A(lsb), in cpu endianness
            PIX_FMT_BGR32_1,   // Packed RGB 8:8:8, 32bpp, (msb)8B 8G 8R 8A(lsb), in cpu endianness

            PIX_FMT_NB,        // number of pixel
        };


        [StructLayout(LayoutKind.Sequential)]
        public class ByteIOContext
        {
            IntPtr buffer;

            [MarshalAs(UnmanagedType.I4)]
            int buffer_size;

            IntPtr buf_ptr;

            IntPtr buf_end;

            //[MarshalAs(UnmanagedType.FunctionPtr)]
            //AnonymousCallback opaque;
            IntPtr opaque;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            Read_PacketCallback read_packet;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            WritePacketCallback write_packet;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            SeekCallback seek;

            [MarshalAs(UnmanagedType.I8)]
            Int64 pos; // position in the file of the current buffer 

            [MarshalAs(UnmanagedType.I4)]
            int must_flush; // true if the next seek should flush

            [MarshalAs(UnmanagedType.I4)]
            int eof_reached; // true if eof reached

            [MarshalAs(UnmanagedType.I4)]
            int write_flag;  // true if open for writing 

            [MarshalAs(UnmanagedType.I4)]
            int is_streamed;

            [MarshalAs(UnmanagedType.I4)]
            int max_packet_size;

            [MarshalAs(UnmanagedType.U4)]
            uint checksum;

            IntPtr checksum_ptr;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            UpdateChecksumCallback update_checksum;

            [MarshalAs(UnmanagedType.I4)]
            int error; // contains the error code or 0 if no error happened
        };

        public delegate String ItemNameCallback();

        [StructLayout(LayoutKind.Sequential)]
        public struct AVClass
        {
            [MarshalAs(UnmanagedType.LPStr)]
            String class_name;
            ItemNameCallback item_name;
            IntPtr pAVOption;
        };

        public struct AVOption
        {
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct AVRational
        {
            [MarshalAs(UnmanagedType.I4)]
            public int num;
            [MarshalAs(UnmanagedType.I4)]
            public int den;
        };
    }
}

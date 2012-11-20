using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using SharpFFmpeg;

namespace test
{
  class Program
  {
    static void Main(string[] args)
    {
      FFmpeg.avcodec_register_all();
      FFmpeg.avdevice_register_all();
      FFmpeg.av_register_all();

      IntPtr avformat_optsPtr = FFmpeg.av_alloc_format_context();

      ShowVersion("avcodec", FFmpeg.avcodec_version());
      ShowVersion("avdevice", FFmpeg.avdevice_version());
      ShowVersion("avformat", FFmpeg.avformat_version());
      ShowVersion("avutil", FFmpeg.avutil_version());
      
      ShowFormats();

      Console.Read();
    }

    static void ShowBanner()
    {
      Console.WriteLine("");
    }

    static void ShowVersion(string libName, uint version)
    {
      Console.WriteLine("lib{0,-9}{1,10}.{2,2}.{3,2}", libName, version >> 16, version >> 8 & 0xff, version & 0xff);
    }

    static void ShowFormats()
    {
      Console.WriteLine("File formats:");

      IntPtr ifmtPtr = IntPtr.Zero;
      FFmpeg.AVInputFormat ifmt;
      IntPtr ofmtPtr = IntPtr.Zero;
      FFmpeg.AVOutputFormat ofmt;

      string last_name = "";

      while (true)
      {
        bool encode = false;
        bool decode = false;
        string name = null;
        string long_name = null;

        while ((ofmtPtr = FFmpeg.av_oformat_next(ofmtPtr)) != IntPtr.Zero)
        {
          ofmt = (FFmpeg.AVOutputFormat)Marshal.PtrToStructure(ofmtPtr, typeof(FFmpeg.AVOutputFormat));
          if ((name == null
              ||
              String.Compare(ofmt.name, name, StringComparison.InvariantCulture) < 0)
              && String.Compare(ofmt.name, last_name, StringComparison.InvariantCulture) > 0)
          {
            name = ofmt.name;
            long_name = ofmt.long_name;
            encode = true;
          }          
        }

        while ((ifmtPtr = FFmpeg.av_iformat_next(ifmtPtr)) != IntPtr.Zero)
        {
          ifmt = (FFmpeg.AVInputFormat)Marshal.PtrToStructure(ifmtPtr, typeof(FFmpeg.AVInputFormat));
          if ((name == null
              ||
              String.Compare(ifmt.name, name, StringComparison.InvariantCulture) < 0)
              && String.Compare(ifmt.name, last_name, StringComparison.InvariantCulture) > 0)
          {
            name = ifmt.name;
            long_name = ifmt.long_name;
            encode = false;            
          }
          if (name != null && string.Compare(ifmt.name, name) == 0)
          {
            decode = true;
          }
        }

        if (name == null)
        {
          break;
        }
        last_name = name;

        Console.WriteLine(String.Format("{0}{1} {2,-15} {3}", decode ? "D" : " ", encode ? "E" : " ", name, long_name));
      }

      Console.WriteLine();

      Console.WriteLine("Codecs:");

      last_name = "";

      IntPtr pPtr = IntPtr.Zero;
      FFmpeg.AVCodec p = new FFmpeg.AVCodec();
      IntPtr p2Ptr = IntPtr.Zero;
      FFmpeg.AVCodec p2 = new FFmpeg.AVCodec();

      while(true)
      {
        bool decode = false;
        bool encode = false;
        int cap=0;
        string type_str;

        p2Ptr = IntPtr.Zero;
        while((pPtr = FFmpeg.av_codec_next(pPtr)) != IntPtr.Zero) 
        {
          p = (FFmpeg.AVCodec)Marshal.PtrToStructure(pPtr, typeof(FFmpeg.AVCodec));
            if((p2Ptr == IntPtr.Zero || string.Compare(p.name, p2.name) < 0) &&
                string.Compare(p.name, last_name) > 0)
            {
              p2 = p;
              p2Ptr = pPtr;
              decode = false;
              encode = false;
              cap = 0;
            }
            if (p2Ptr != IntPtr.Zero && string.Compare(p.name, p2.name) == 0)
            {
              if(p.decode != null) decode = true;
              if (p.encode != null) encode = true;
              cap |= p.capabilities;
            }
        }
        if(p2Ptr == IntPtr.Zero)
            break;
        last_name= p2.name;

        switch(p2.type) {
        case FFmpeg.CodecType.CODEC_TYPE_VIDEO:
            type_str = "V";
            break;
        case FFmpeg.CodecType.CODEC_TYPE_AUDIO:
            type_str = "A";
            break;
        case FFmpeg.CodecType.CODEC_TYPE_SUBTITLE:
            type_str = "S";
            break;
        default:
            type_str = "?";
            break;
        }
        Console.WriteLine(String.Format(
            " {0}{1}{2}{3}{4}{5} {6,-15} {7}",
            decode ? "D": (/*p2->decoder ? "d":*/" "),
            encode ? "E":" ",
            type_str,
            (cap & FFmpeg.CODEC_CAP_DRAW_HORIZ_BAND) > 0 ? "S":" ",
            (cap & FFmpeg.CODEC_CAP_DR1) > 0 ? "D":" ",
            (cap & FFmpeg.CODEC_CAP_TRUNCATED) > 0 ? "T":" ",
            p2.name,
            p2.long_name ?? ""));
       /* if(p2->decoder && decode==0)
            printf(" use %s for decoding", p2->decoder->name);*/
      }

      Console.WriteLine();

      IntPtr bsfPtr = IntPtr.Zero;
      FFmpeg.AVBitStreamFilter bsf = new FFmpeg.AVBitStreamFilter();

      Console.WriteLine("Bitstream filters:");
      while((bsfPtr = FFmpeg.av_bitstream_filter_next(bsfPtr)) != IntPtr.Zero)
      {
        bsf = (FFmpeg.AVBitStreamFilter)Marshal.PtrToStructure(bsfPtr, typeof(FFmpeg.AVBitStreamFilter));
        Console.Write(" {0}", bsf.name);
      }
      Console.WriteLine();
      Console.WriteLine();
      
      /*
      IntPtr upPtr = IntPtr.Zero;
      FFmpeg.URLProtocol up = new FFmpeg.URLProtocol();

      Console.WriteLine("Supported file protocols:");
      while ((bsfPtr = FFmpeg.av_protocol_next(bsfPtr)) != IntPtr.Zero)
      {
        bsf = (FFmpeg.URLProtocol)Marshal.PtrToStructure(bsfPtr, typeof(FFmpeg.URLProtocol));
        Console.Write(" {0}:", up.name);
      }
      Console.WriteLine();
      Console.WriteLine();
      */

      Console.WriteLine(
        "Frame size, frame rate abbreviations:\n ntsc pal qntsc qpal sntsc spal film ntsc-film sqcif qcif cif 4cif");
      Console.WriteLine();

      Console.WriteLine("Note, the names of encoders and decoders do not always match, so there are");
      Console.WriteLine("several cases where the above table shows encoder only or decoder only entries");
      Console.WriteLine("even though both encoding and decoding are supported. For example, the h263");
      Console.WriteLine("decoder corresponds to the h263 and h263p encoders, for file formats it is even");
      Console.WriteLine("worse.");

    }

    
    
  }
}

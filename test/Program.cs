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

      ShowBanner();

      //ShowFormats();
      InputFile("test.ogg");

      Console.Read();
    }

    static void InputFile(string filename)
    {
      var avformatcontextPtr = FFmpeg.av_alloc_format_context();
      var avformatparameters = new FFmpeg.AVFormatParameters()
        {
          prealloced_context = 1,
          sample_rate = 0,
          channels = 0,
          width = 0,
          height = 0,
          pix_fmt = 0,
          channel = 0,
          standard = string.Empty,
          video_codec_id = 0
        };

      var avformatparametersPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FFmpeg.AVFormatParameters)));
      Marshal.StructureToPtr(avformatparameters, avformatparametersPtr, false);
      var err = FFmpeg.av_open_input_file(out avformatcontextPtr, filename, IntPtr.Zero, 0, avformatparametersPtr);

      if (err < 0)
      {
        Console.WriteLine(string.Format("Error: {0}", err));
        return;
      }

      var ret = FFmpeg.av_find_stream_info(avformatcontextPtr);

      if (ret < 0)
      {
        Console.WriteLine("could not find codec parameters");
      }

      DumpFormat(avformatcontextPtr, 0, filename, false);

      Marshal.FreeHGlobal(avformatparametersPtr);
      FFmpeg.av_freep(avformatcontextPtr);
    }

    static void DumpFormat(IntPtr avformatcontextPtr, int index, string filename, bool isOutput)
    {
      var ic = (FFmpeg.AVFormatContext)Marshal.PtrToStructure(avformatcontextPtr, typeof(FFmpeg.AVFormatContext));
      var format = Marshal.PtrToStructure(
        isOutput ? ic.oformat : ic.iformat, isOutput ? typeof(FFmpeg.AVOutputFormat) : typeof(FFmpeg.AVInputFormat));
      Console.WriteLine(string.Format("{0} #{1}, {2}, {3} '{4}':",
        isOutput ? "Output" : "Input",
        index,
        isOutput ? ((FFmpeg.AVOutputFormat)format).name : ((FFmpeg.AVInputFormat)format).name,
        isOutput ? "to" : "from",
        filename));

      if (!isOutput)
      {
        Console.Write("  Duration: ");
        if (ic.duration != -1)
        {
          int hours, mins, secs, us;
          secs = (int)(ic.duration / FFmpeg.AV_TIME_BASE);
          us = (int)(ic.duration % FFmpeg.AV_TIME_BASE);
          mins = secs / 60;
          secs %= 60;
          hours = mins / 60;
          mins %= 60;
          Console.Write(string.Format("{0:00}:{1:00}:{2:00}.{3:00}", hours, mins, secs, (100 * us) / FFmpeg.AV_TIME_BASE));
        }
        else
        {
          Console.Write("N/A");
        }
        if (ic.start_time != -1)
        {
          long secs, us;
          Console.Write(", start: ");
          secs = ic.start_time / FFmpeg.AV_TIME_BASE;
          us = ic.start_time % FFmpeg.AV_TIME_BASE;
          Console.Write("{0}.{1:000000}", secs, FFmpeg.av_rescale(us, 1000000, FFmpeg.AV_TIME_BASE));
        }
        Console.Write(", bitrate: ");
        if (ic.bit_rate != 0)
        {
          Console.Write("{0} kb/s", ic.bit_rate / 1000);
        }
        else
        {
          Console.Write("N/A");
        }
        Console.WriteLine();
        for (int i = 0; i < ic.nb_streams; i++)
        {
          DumpStreamFormat(avformatcontextPtr, ic, format, i, index, isOutput);
        }
      }
    }

    static void DumpStreamFormat(IntPtr icPtr, FFmpeg.AVFormatContext ic, object format, int i, int index, bool isOutput)
    {
      var flags = isOutput ? ((FFmpeg.AVOutputFormat)format).flags : ((FFmpeg.AVInputFormat)format).flags;
      var st = (FFmpeg.AVStream)Marshal.PtrToStructure(ic.streams[i], typeof(FFmpeg.AVStream));
      int g = (int)FFmpeg.ff_gcd(st.time_base.num, st.time_base.den);

      var buf = new StringBuilder(256);

      FFmpeg.avcodec_string(buf, buf.Capacity, st.codec, isOutput ? 1 : 0);
      Console.Write("    Stream #{0}.{1}", index, i);
      if ((flags & FFmpeg.AVFMT_SHOW_IDS) != 0)
      {
        Console.Write("[0x{0:x}]", st.id);
      }
      if (st.language.Length > 0)
      {
        Console.Write("({0})", st.language);
      }
      Console.Write(", {0}/{1}", st.time_base.num / g, st.time_base.den / g);
      Console.Write(": {0}", buf);
      var codec = (FFmpeg.AVCodecContext)Marshal.PtrToStructure(st.codec, typeof(FFmpeg.AVCodecContext));
      if (codec.codec_type == FFmpeg.CodecType.CODEC_TYPE_VIDEO)
      {
        if (st.r_frame_rate.den != 0 && st.r_frame_rate.num != 0)
        {
          Console.Write(", {0:f} tb(r)", FFmpeg.av_q2d(st.r_frame_rate));
        }
        else
        {
          Console.Write(", {0:f} tb(c)", 1 / FFmpeg.av_q2d(codec.time_base));
        }
      }
      Console.WriteLine();
    }

    static void ShowBanner()
    {
      Console.WriteLine("FFmpeg with SharpFFmpeg");
      ShowVersion("avutil", FFmpeg.avutil_version());
      ShowVersion("avcodec", FFmpeg.avcodec_version());
      ShowVersion("avformat", FFmpeg.avformat_version());
      ShowVersion("avdevice", FFmpeg.avdevice_version());      
    }

    static void ShowVersion(string libName, uint version)
    {
      Console.WriteLine("lib{0,-9}{1,10}.{2,2}.{3,2}", 
        libName, 
        version >> 16, 
        version >> 8 & 0xff, 
        version & 0xff);
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

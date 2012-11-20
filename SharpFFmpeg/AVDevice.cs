using System.Runtime.InteropServices;
using System.Security;

namespace SharpFFmpeg
{
  public partial class FFmpeg
  {
    [DllImport(AVDEVICE_DLL_NAME), SuppressUnmanagedCodeSecurity]
    public static extern uint avdevice_version();

    [DllImport(AVDEVICE_DLL_NAME), SuppressUnmanagedCodeSecurity]
    public static extern void avdevice_register_all();

  }
}

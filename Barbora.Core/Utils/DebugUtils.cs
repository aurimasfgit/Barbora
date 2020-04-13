using System;
using System.Diagnostics;

namespace Barbora.Core.Utils
{
    public static class DebugUtils
    {
        public static void WriteLineToDebugConsole(string message)
        {
            Debug.WriteLine(string.Format("{0} -> {1}", BaseUtils.GetNowDateTime(), message));
        }

        public static void WriteLineToDebugConsole(this Exception exc)
        {
            WriteLineToDebugConsole(exc?.InnerException?.Message ?? exc?.Message);
        }
    }
}
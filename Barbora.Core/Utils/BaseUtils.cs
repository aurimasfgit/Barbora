using System;

namespace Barbora.Core.Utils
{
    public static class BaseUtils
    {
        public static string GetNowDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
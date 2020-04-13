using Barbora.Core.Utils;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace Barbora.App.Helpers
{
    public static class AuthCookieHelper
    {
        private static string GetAuthCookiePath()
        {
            var tempPath = Path.GetTempPath();
            var authCookiePath = Path.Combine(tempPath, "BRBAUTH");

            return authCookiePath;
        }

        public static Cookie GetAuthCookie()
        {
            try
            {
                var authCookiePath = GetAuthCookiePath();

                if (File.Exists(authCookiePath))
                {
                    var json = File.ReadAllText(authCookiePath);

                    if (!string.IsNullOrEmpty(json))
                        return JsonConvert.DeserializeObject<Cookie>(json);
                }
            }
            catch (Exception exc)
            {
                exc.WriteLineToDebugConsole();
            }

            return null;
        }

        public static void SetAuthCookie(Cookie authCookie)
        {
            try
            {
                var json = JsonConvert.SerializeObject(authCookie);

                if (!string.IsNullOrEmpty(json))
                    File.WriteAllText(GetAuthCookiePath(), json);
            }
            catch (Exception exc)
            {
                exc.WriteLineToDebugConsole();
            }
        }

        public static void RemoveAuthCookie()
        {
            try
            {
                File.Delete(GetAuthCookiePath());
            }
            catch (Exception exc)
            {
                exc.WriteLineToDebugConsole();
            }
        }
    }
}
using System;
using System.IO;
using System.Reflection;

namespace Barbora.App.Helpers
{
    public static class ResourceHelper
    {
        private static readonly string assemblyName = "Barbora.App";
        private static readonly Assembly assembly = typeof(App).GetTypeInfo().Assembly;

        public static Stream GetResourceStream(string resourceName)
        {
            var fullName = string.Format("{0}.{1}", assemblyName, resourceName);
            var resourceStream = assembly.GetManifestResourceStream(fullName);

            if (resourceStream == null)
                throw new ArgumentNullException("resourceStream");

            return resourceStream;
        }
    }
}
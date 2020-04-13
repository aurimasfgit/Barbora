using Barbora.Core.Utils;
using System;

namespace Barbora.App.Services
{
    public interface IExceptionHandlingService
    {
        void LogUnhandledException(Exception exception, string source);
    }

    public class ExceptionHandlingService : IExceptionHandlingService
    {
        public void LogUnhandledException(Exception exception, string source)
        {
            DebugUtils.WriteLineToDebugConsole($"Unhandled exception in ({source})");

            // TODO: [add logging]
            switch (exception?.InnerException ?? exception)
            {
                case Exception e:
                    e.WriteLineToDebugConsole();
                    break;
            }
        }
    }
}
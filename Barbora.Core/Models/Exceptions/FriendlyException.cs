using System;
using System.Diagnostics;

namespace Barbora.Core.Models.Exceptions
{
    public class FriendlyException : Exception
    {
        public FriendlyException(string message)
            : base(message)
        {
            Debug.WriteLine(message);
        }
    }
}
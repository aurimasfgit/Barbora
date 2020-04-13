using System.Collections.Generic;

namespace Barbora.Core.Models
{
    public class Message
    {
        public IList<MessageInfo> error { get; set; }
        public IList<MessageInfo> warning { get; set; }
        public IList<MessageInfo> info { get; set; }
        public IList<MessageInfo> success { get; set; }

        public object data { get; set; }

        public MessageInfo GetFirstError()
        {
            return error?[0];
        }

        public string GetFirstErrorMessage()
        {
            return GetFirstError()?.message;
        }

        public string GetFirstWarningMessage()
        {
            return warning?[0]?.message;
        }

        public string GetFirstInfoMessage()
        {
            return info?[0]?.message;
        }

        public string GetFirstSuccessMessage()
        {
            return success?[0]?.message;
        }
    }

    public class MessageInfo
    {
        public string Id { get; set; }

        public string message { get; set; }
        public object data { get; set; }
    }
}
using System;

namespace EventBasedTCP
{
    public class ResponseEvent
    {
        public string Content { get; set; }
        public ContentMode Mode { get; set; }
        public Action<MessageReceivedEventArgs> Event { get; set; }
    }
}
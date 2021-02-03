using System;
using System.Collections.Generic;
using System.Text;

namespace MK94.SeeRaw
{
    public class Message
    {
        public MessageType Type { get; set; }
    }

    public class ExecuteMessage : Message
    {
        public string Id { get; set; }
        public object[] Args { get; set; }
    }

    public enum MessageType
    {
        Execute,
    }
}

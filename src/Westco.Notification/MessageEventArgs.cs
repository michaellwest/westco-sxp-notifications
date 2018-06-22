using System;

namespace Westco.Notification
{
    public class MessageEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Icon { get; set; }
        public string SessionId { get; set; }
        public bool CanBroadcast { get; set; }
    }
}

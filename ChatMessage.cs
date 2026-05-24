using System;

namespace CybersecurityChatbot.Models
{
    public class ChatMessage
    {
        public string Text { get; set; }
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
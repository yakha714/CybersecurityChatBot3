using System.Collections.Generic;

namespace CybersecurityChatbot.Models
{
    public class UserMemory
    {
        public string CurrentTopic { get; set; }
        public int TotalMessages { get; set; }
        public List<string> RecentTopics { get; set; }
        public Dictionary<string, string> UserInfo { get; set; }
        public string LastUserMessage { get; set; }
        public string LastBotResponse { get; set; }
    }
}
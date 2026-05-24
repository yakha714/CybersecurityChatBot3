using System;
using System.Collections.Generic;
using System.Linq;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    public class MemoryManager
    {
        private readonly List<ChatMessage> _history = new();
        private readonly Dictionary<string, string> _userInfo = new();
        private readonly Queue<string> _recentTopics = new();
        private readonly Stack<string> _flow = new();
        private string _currentTopic = "";
        private const int MaxSize = 100;

        public void StoreUserMessage(string msg)
        {
            _history.Add(new ChatMessage { Text = msg, IsUser = true, Timestamp = DateTime.Now });
            _flow.Push("User: " + msg);
            if (_history.Count > MaxSize) _history.RemoveAt(0);
            ExtractTopic(msg);
        }

        public void StoreBotResponse(string resp)
        {
            _history.Add(new ChatMessage { Text = resp, IsUser = false, Timestamp = DateTime.Now });
            _flow.Push("Bot: " + resp);
            if (_history.Count > MaxSize) _history.RemoveAt(0);
        }

        private void ExtractTopic(string msg)
        {
            string low = msg.ToLower();
            string[] topics = { "password", "scam", "privacy", "phishing" };
            foreach (var t in topics)
                if (low.Contains(t)) { _currentTopic = t; _recentTopics.Enqueue(t); if (_recentTopics.Count > 5) _recentTopics.Dequeue(); break; }
        }

        public string GetCurrentTopic() => _currentTopic;
        public bool HasCurrentTopic() => !string.IsNullOrEmpty(_currentTopic);
        public UserMemory GetUserMemory() => new() { CurrentTopic = _currentTopic, TotalMessages = _history.Count, RecentTopics = _recentTopics.ToList(), UserInfo = new(_userInfo), LastUserMessage = GetLastUserMessage(), LastBotResponse = GetLastBotResponse() };
        private string GetLastUserMessage() => _history.LastOrDefault(m => m.IsUser)?.Text;
        private string GetLastBotResponse() => _history.LastOrDefault(m => !m.IsUser)?.Text;
        public void Reset() { _history.Clear(); _userInfo.Clear(); _recentTopics.Clear(); _flow.Clear(); _currentTopic = ""; }
        public string GetConversationFlowText() => string.Join(Environment.NewLine, _flow.Reverse().Take(10));
    }
}
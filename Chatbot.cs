using System;
using System.Threading.Tasks;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    public delegate void ResponseGeneratedEventHandler(string response);
    public delegate void ErrorOccurredEventHandler(string errorMessage);

    public class Chatbot
    {
        public event ResponseGeneratedEventHandler OnResponseGenerated;
        public event ErrorOccurredEventHandler OnErrorOccurred;

        private readonly ResponseManager _responseManager;
        private readonly SentimentAnalyzer _sentimentAnalyzer;
        private readonly MemoryManager _memoryManager;
        private bool _isProcessing;
        private readonly string _currentSessionId;

        public Chatbot()
        {
            _responseManager = new ResponseManager();
            _sentimentAnalyzer = new SentimentAnalyzer();
            _memoryManager = new MemoryManager();
            _isProcessing = false;
            _currentSessionId = Guid.NewGuid().ToString();
            _responseManager.OnTopicChanged += HandleTopicChanged;
        }

        private void HandleTopicChanged(string newTopic) => System.Diagnostics.Debug.WriteLine($"Topic: {newTopic}");

        public async Task<string> ProcessUserInputAsync(string userInput)
        {
            if (_isProcessing) return "Please wait, I'm still processing...";
            if (string.IsNullOrWhiteSpace(userInput)) return "Please enter a message.";

            try
            {
                _isProcessing = true;
                _memoryManager.StoreUserMessage(userInput);
                var sentiment = _sentimentAnalyzer.AnalyzeText(userInput);
                bool isFollowUp = IsFollowUpQuestion(userInput);
                string response = (isFollowUp && _memoryManager.HasCurrentTopic()) ? _responseManager.GetFollowUpResponse(_memoryManager.GetCurrentTopic()) : _responseManager.GetResponse(userInput);
                response = _sentimentAnalyzer.AdaptResponseToSentiment(response, sentiment);
                _memoryManager.StoreBotResponse(response);
                OnResponseGenerated?.Invoke(response);
                return response;
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke(ex.Message);
                return "I encountered an error. Please try again.";
            }
            finally { _isProcessing = false; }
        }

        private bool IsFollowUpQuestion(string input)
        {
            var lower = input.ToLower();
            string[] phrases = { "tell me more", "explain more", "another tip", "more tips", "more details", "elaborate", "continue", "what else", "give me another", "one more tip" };
            foreach (var p in phrases) if (lower.Contains(p)) return true;
            return false;
        }

        public UserMemory GetUserMemory() => _memoryManager.GetUserMemory();
        public void ResetConversation()
        {
            _memoryManager.Reset();
            _responseManager.ResetCurrentTopic();
            OnResponseGenerated?.Invoke("Conversation reset. How can I help?");
        }
        public string GetSessionInfo() => $"Session: {_currentSessionId}\nMessages: {_memoryManager.GetUserMemory().TotalMessages}\nCurrent topic: {_memoryManager.GetCurrentTopic() ?? "None"}";
        public string GetCurrentTopicFromMemory() => _memoryManager.GetCurrentTopic();
        public string GetConversationFlow() => _memoryManager.GetConversationFlowText();
    }
}
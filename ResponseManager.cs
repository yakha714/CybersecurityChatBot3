using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot.Services
{
    public delegate void TopicChangedEventHandler(string newTopic);

    public class ResponseManager
    {
        public event TopicChangedEventHandler OnTopicChanged;
        private readonly Dictionary<string, List<string>> _keywordResponses;
        private readonly Dictionary<string, List<string>> _randomResponseLists;
        private readonly Dictionary<string, List<string>> _followUpResponses;
        private readonly List<string> _defaultResponses;
        private readonly Random _random;
        private string _currentTopic;
        private readonly List<string> _topicPriority = new List<string> { "privacy", "scam", "phishing", "password", "help" };

        public ResponseManager()
        {
            _random = new Random();
            _currentTopic = "";
            _keywordResponses = new Dictionary<string, List<string>>();
            _randomResponseLists = new Dictionary<string, List<string>>();
            _followUpResponses = new Dictionary<string, List<string>>();
            _defaultResponses = new List<string>();
            InitializeKeywordResponses();
            InitializeRandomResponses();
            InitializeFollowUpResponses();
            InitializeDefaultResponses();
        }

        private void InitializeKeywordResponses()
        {
            _keywordResponses["privacy"] = new List<string>
            {
                "[PRIVACY PROTECTION] Review your social media privacy settings regularly. Limit what personal information you share publicly.",
                "[PRIVACY PROTECTION] Use a VPN on public Wi-Fi networks to encrypt your internet traffic and protect your sensitive data.",
                "[PRIVACY PROTECTION] Regularly audit which apps have access to your camera, microphone, and location. Revoke unnecessary permissions."
            };
            _keywordResponses["scam"] = new List<string>
            {
                "[SCAM DETECTION] Be cautious of unsolicited emails, calls, or texts asking for personal information. Scammers often create urgency.",
                "[SCAM DETECTION] If something sounds too good to be true, it probably is a scam. Verify through official channels.",
                "[SCAM DETECTION] Never click links in suspicious messages. Type the official website address directly into your browser."
            };
            _keywordResponses["phishing"] = new List<string>
            {
                "[PHISHING PREVENTION] Check email sender addresses carefully for look-alike domains. Scammers change one letter to fool you.",
                "[PHISHING PREVENTION] Hover over links before clicking to see the actual URL. Don't click if it looks suspicious.",
                "[PHISHING PREVENTION] Look for red flags like urgent language, grammatical errors, and requests for personal information."
            };
            _keywordResponses["password"] = new List<string>
            {
                "[PASSWORD SAFETY] Use strong, unique passwords for each account. Combine uppercase, lowercase, numbers, and special characters.",
                "[PASSWORD SAFETY] Enable two-factor authentication whenever possible for an extra layer of security.",
                "[PASSWORD SAFETY] Never share your passwords. Legitimate organizations will never ask for your password."
            };
            _keywordResponses["help"] = new List<string>
            {
                "[HELP] I can help with: password safety, scam detection, privacy protection, and phishing prevention. Ask me about any of these!"
            };
        }

        private void InitializeRandomResponses()
        {
            _randomResponseLists["phishing_tips"] = new List<string> { "Enable email filters to reduce spam and phishing attempts.", "Use a password manager that only fills passwords on legitimate sites.", "Report phishing attempts to the company being impersonated.", "Keep your browser updated for protection against known phishing sites." };
            _randomResponseLists["password_tips"] = new List<string> { "Consider using passphrases - a sequence of random words.", "Change your passwords every 3-6 months.", "Never reuse passwords across different services.", "Avoid common passwords like '123456' or 'password'." };
            _randomResponseLists["scam_tips"] = new List<string> { "Be wary of text messages claiming package delivery problems.", "Job scams often ask for payment upfront or sensitive information.", "Investment scams use pressure tactics like 'limited time offers'.", "If someone demands payment via gift cards, it's a scam." };
            _randomResponseLists["privacy_tips"] = new List<string> { "Use private browsing mode on shared computers.", "Consider using email aliases for different services.", "Check if your email has been in a data breach.", "Be careful what you post online - attackers can use it." };
        }

        private void InitializeFollowUpResponses()
        {
            _followUpResponses["privacy"] = new List<string> { "[MORE ON PRIVACY] Opt out of data brokers that collect your personal information.", "[MORE ON PRIVACY] Use end-to-end encrypted messaging for sensitive conversations." };
            _followUpResponses["scam"] = new List<string> { "[MORE ON SCAMS] Search online using '[company] scam' before responding.", "[MORE ON SCAMS] Keep records of suspicious communications to report them." };
            _followUpResponses["phishing"] = new List<string> { "[MORE ON PHISHING] If you click a phishing link, disconnect from the internet immediately.", "[MORE ON PHISHING] Enable multi-factor authentication on your email account." };
            _followUpResponses["password"] = new List<string> { "[MORE ON PASSWORDS] Use a password manager to generate and store strong passwords.", "[MORE ON PASSWORDS] Biometric authentication adds convenience and security." };
        }

        private void InitializeDefaultResponses()
        {
            _defaultResponses.Add("I'm not sure I understand. Can you try rephrasing? Ask me about passwords, scams, privacy, or phishing.");
            _defaultResponses.Add("I want to help, but I'm having trouble understanding. Please ask about password safety, scam detection, privacy protection, or phishing.");
            _defaultResponses.Add("I didn't quite catch that. Please ask a specific question about passwords, scams, privacy, or phishing.");
        }

        public string GetResponse(string userInput)
        {
            string lower = userInput.ToLower();
            if (lower.Contains("fishing")) lower = lower.Replace("fishing", "phishing");
            if (lower.Contains("privicy")) lower = lower.Replace("privicy", "privacy");
            foreach (var topic in _topicPriority)
            {
                if (lower.Contains(topic))
                {
                    _currentTopic = topic;
                    OnTopicChanged?.Invoke(topic);
                    return _keywordResponses[topic][_random.Next(_keywordResponses[topic].Count)];
                }
            }
            return GetDefaultResponse();
        }

        public string GetRandomResponseForTopic(string topic)
        {
            if (string.IsNullOrEmpty(topic)) return GetDefaultResponse();
            if (topic.Contains("privacy") && _randomResponseLists.ContainsKey("privacy_tips")) return _randomResponseLists["privacy_tips"][_random.Next(_randomResponseLists["privacy_tips"].Count)];
            if (topic.Contains("scam") && _randomResponseLists.ContainsKey("scam_tips")) return _randomResponseLists["scam_tips"][_random.Next(_randomResponseLists["scam_tips"].Count)];
            if (topic.Contains("phishing") && _randomResponseLists.ContainsKey("phishing_tips")) return _randomResponseLists["phishing_tips"][_random.Next(_randomResponseLists["phishing_tips"].Count)];
            if (topic.Contains("password") && _randomResponseLists.ContainsKey("password_tips")) return _randomResponseLists["password_tips"][_random.Next(_randomResponseLists["password_tips"].Count)];
            return _randomResponseLists.Values.SelectMany(v => v).ToList()[_random.Next(_randomResponseLists.Values.SelectMany(v => v).Count())];
        }

        public string GetFollowUpResponse(string topic)
        {
            if (_followUpResponses.ContainsKey(topic)) return _followUpResponses[topic][_random.Next(_followUpResponses[topic].Count)];
            return GetRandomResponseForTopic(topic);
        }
        public string GetDefaultResponse() => _defaultResponses[_random.Next(_defaultResponses.Count)];
        public void ResetCurrentTopic() => _currentTopic = "";
        public string GetCurrentTopic() => _currentTopic;
    }
}
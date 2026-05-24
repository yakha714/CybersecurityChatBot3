using System;
using System.Collections.Generic;
using System.Linq;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.Services
{
    public class SentimentAnalyzer
    {
        private readonly Dictionary<string, int> _positiveWords;
        private readonly Dictionary<string, int> _negativeWords;
        private readonly Dictionary<string, int> _angryWords;
        private readonly Dictionary<string, int> _frustratedWords;
        private readonly List<string> _intensifiers;

        public double CurrentSentimentScore { get; private set; }
        public SentimentType LastDetectedSentiment { get; private set; }

        public SentimentAnalyzer()
        {
            _positiveWords = new Dictionary<string, int>();
            _negativeWords = new Dictionary<string, int>();
            _angryWords = new Dictionary<string, int>();
            _frustratedWords = new Dictionary<string, int>();
            _intensifiers = new List<string>();
            CurrentSentimentScore = 0.0;
            LastDetectedSentiment = SentimentType.Neutral;
            InitializeWordDictionaries();
        }

        private void InitializeWordDictionaries()
        {
            _positiveWords.Add("thanks", 2); _positiveWords.Add("thank", 2);
            _positiveWords.Add("good", 1); _positiveWords.Add("great", 2);
            _positiveWords.Add("helpful", 2); _positiveWords.Add("awesome", 3);
            _positiveWords.Add("perfect", 2); _positiveWords.Add("excellent", 3);

            _negativeWords.Add("bad", 2); _negativeWords.Add("worried", 2);
            _negativeWords.Add("concerned", 2); _negativeWords.Add("scared", 3);
            _negativeWords.Add("unsafe", 3); _negativeWords.Add("nervous", 2);

            _angryWords.Add("angry", 3); _angryWords.Add("mad", 2);
            _angryWords.Add("upset", 2); _angryWords.Add("furious", 4);
            _angryWords.Add("hate", 3);

            _frustratedWords.Add("confused", 2); _frustratedWords.Add("frustrated", 3);
            _frustratedWords.Add("not working", 2); _frustratedWords.Add("don't understand", 2);
            _frustratedWords.Add("complicated", 2);

            _intensifiers.Add("very"); _intensifiers.Add("really");
            _intensifiers.Add("extremely"); _intensifiers.Add("so"); _intensifiers.Add("too");
        }

        public SentimentResult AnalyzeText(string text)
        {
            string lowerText = text.ToLower();
            int positiveScore = CalculateWordScore(lowerText, _positiveWords);
            int negativeScore = CalculateWordScore(lowerText, _negativeWords);
            int angryScore = CalculateWordScore(lowerText, _angryWords);
            int frustratedScore = CalculateWordScore(lowerText, _frustratedWords);

            double multiplier = CalculateIntensifierMultiplier(lowerText);
            positiveScore = (int)(positiveScore * multiplier);
            negativeScore = (int)(negativeScore * multiplier);
            angryScore = (int)(angryScore * multiplier);
            frustratedScore = (int)(frustratedScore * multiplier);

            SentimentType primary = SentimentType.Neutral;
            int highest = 0;
            if (positiveScore > highest) { highest = positiveScore; primary = SentimentType.Positive; }
            if (negativeScore > highest) { highest = negativeScore; primary = SentimentType.Negative; }
            if (angryScore > highest) { highest = angryScore; primary = SentimentType.Angry; }
            if (frustratedScore > highest) { highest = frustratedScore; primary = SentimentType.Frustrated; }

            double confidence = Math.Min(1.0, highest / 10.0);
            CurrentSentimentScore = highest;
            LastDetectedSentiment = primary;

            return new SentimentResult
            {
                SentimentType = primary,
                Confidence = confidence,
                EmotionalResponse = GetEmotionalResponse(primary),
                SentimentColor = GetSentimentColor(primary),
                DetectedWords = GetDetectedWords(lowerText),
                Score = highest,
                AnalysisTime = DateTime.Now
            };
        }

        private int CalculateWordScore(string text, Dictionary<string, int> dict)
        {
            int score = 0;
            foreach (var kvp in dict)
                if (text.Contains(kvp.Key))
                    score += kvp.Value;
            return score;
        }

        private double CalculateIntensifierMultiplier(string text)
        {
            double mult = 1.0;
            foreach (string word in _intensifiers)
                if (text.Contains(word))
                    mult += 0.5;
            return Math.Min(mult, 2.0);
        }

        private string GetEmotionalResponse(SentimentType sentiment)
        {
            switch (sentiment)
            {
                case SentimentType.Positive: return "I'm glad you're engaging positively with cybersecurity.";
                case SentimentType.Negative: return "I understand security concerns can be stressful. Let me help.";
                case SentimentType.Angry: return "I hear your frustration. Let me provide clear, direct information.";
                case SentimentType.Frustrated: return "I apologize if this is confusing. Let me simplify.";
                default: return "I'm here to help with your cybersecurity questions.";
            }
        }

        private string GetSentimentColor(SentimentType sentiment)
        {
            switch (sentiment)
            {
                case SentimentType.Positive: return "#2ECC71";
                case SentimentType.Negative: return "#F39C12";
                case SentimentType.Angry: return "#E74C3C";
                case SentimentType.Frustrated: return "#E67E22";
                default: return "#3498DB";
            }
        }

        private string GetDetectedWords(string text)
        {
            var detected = new List<string>();
            foreach (var kvp in _positiveWords) if (text.Contains(kvp.Key)) detected.Add(kvp.Key);
            foreach (var kvp in _negativeWords) if (text.Contains(kvp.Key)) detected.Add(kvp.Key);
            foreach (var kvp in _angryWords) if (text.Contains(kvp.Key)) detected.Add(kvp.Key);
            foreach (var kvp in _frustratedWords) if (text.Contains(kvp.Key)) detected.Add(kvp.Key);
            return detected.Count > 0 ? string.Join(", ", detected) : "None";
        }

        public string AdaptResponseToSentiment(string original, SentimentResult sentiment)
        {
            if (sentiment.Confidence < 0.3) return original;
            string prefix = "";
            switch (sentiment.SentimentType)
            {
                case SentimentType.Positive: prefix = "Thank you for your positive attitude. "; break;
                case SentimentType.Negative: prefix = "I understand your concerns. "; break;
                case SentimentType.Angry: prefix = "I hear your frustration. Let me help clearly. "; break;
                case SentimentType.Frustrated: prefix = "I see this is challenging. Let me simplify: "; break;
            }
            return prefix + original;
        }

        public bool IsStrongSentiment() => CurrentSentimentScore >= 5 && LastDetectedSentiment != SentimentType.Neutral;
    }
}
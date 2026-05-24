using System;

namespace CybersecurityChatbot.Models
{
    public enum SentimentType { Neutral, Positive, Negative, Angry, Frustrated }

    public class SentimentResult
    {
        public SentimentType SentimentType { get; set; }
        public double Confidence { get; set; }
        public string EmotionalResponse { get; set; }
        public string SentimentColor { get; set; }
        public string DetectedWords { get; set; }
        public int Score { get; set; }
        public DateTime AnalysisTime { get; set; }

        public SentimentResult()
        {
            SentimentType = SentimentType.Neutral;
            Confidence = 0.5;
            EmotionalResponse = "";
            SentimentColor = "#3498DB";
            DetectedWords = "";
            Score = 0;
            AnalysisTime = DateTime.Now;
        }

        public override string ToString() => $"{SentimentType} (Confidence: {Confidence:P0})";
    }
}
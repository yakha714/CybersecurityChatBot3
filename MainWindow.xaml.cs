using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using CybersecurityChatbot.Services;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private Chatbot _chatbot;
        private SentimentAnalyzer _sentimentAnalyzer;
        private int _messageCount;
        private SpeechSynthesizer _speechSynthesizer;
        private SpeechRecognitionEngine _speechRecognizer;

        public MainWindow()
        {
            InitializeComponent();
            _chatbot = new Chatbot();
            _sentimentAnalyzer = new SentimentAnalyzer();
            _messageCount = 0;

            _chatbot.OnResponseGenerated += HandleResponseGenerated;
            _chatbot.OnErrorOccurred += HandleErrorOccurred;

            InitializeVoice();
            LoadAsciiArt();
            UpdateMessageCount();
            UpdateMemoryStatus(true);

            // WELCOME GREETING - this is the part you asked about
            AddBotMessage("Hello. I am your Cybersecurity Awareness Assistant.");
            AddBotMessage("");
            AddBotMessage("I can help you with the following topics:");
            AddBotMessage("  * Password safety");
            AddBotMessage("  * Scam detection");
            AddBotMessage("  * Privacy protection");
            AddBotMessage("  * Phishing prevention");
            AddBotMessage("");
            AddBotMessage("What would you like to know about?");
            SpeakText("Hello. I am your Cybersecurity Awareness Assistant. How can I help you stay safe online today?");
        }

        private void InitializeVoice()
        {
            try
            {
                _speechSynthesizer = new SpeechSynthesizer();
                _speechSynthesizer.SetOutputToDefaultAudioDevice();

                _speechRecognizer = new SpeechRecognitionEngine();
                _speechRecognizer.SetInputToDefaultAudioDevice();
                var grammar = new DictationGrammar();
                _speechRecognizer.LoadGrammar(grammar);
                _speechRecognizer.SpeechRecognized += SpeechRecognizer_SpeechRecognized;
            }
            catch (Exception ex)
            {
                AddBotMessage($"[VOICE] Voice initialization failed: {ex.Message}");
                AddBotMessage("[VOICE] Voice features disabled. You can still type.");
            }
        }

        private void SpeechRecognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UserInput.Text = e.Result.Text;
                SendMessage();
            });
        }

        private void SpeakText(string text)
        {
            try
            {
                _speechSynthesizer?.SpeakAsync(text);
            }
            catch { /* ignore */ }
        }

        private void LoadAsciiArt()
        {
            StringBuilder art = new StringBuilder();
            art.AppendLine("    ================================================================");
            art.AppendLine("    |                      CYBER SECURITY SHIELD                    |");
            art.AppendLine("    |                                                              |");
            art.AppendLine("    |                        /\\---/\\                              |");
            art.AppendLine("    |                       /       \\                             |");
            art.AppendLine("    |                      |  STOP   |                            |");
            art.AppendLine("    |                       \\       /                             |");
            art.AppendLine("    |                        \\/---\\/                              |");
            art.AppendLine("    |                                                              |");
            art.AppendLine("    |           'Stay Safe, Stay Secure Online'                   |");
            art.AppendLine("    ================================================================");
            AsciiArtDisplay.Text = art.ToString();
        }

        private void HandleResponseGenerated(string response)
        {
            Dispatcher.Invoke(() =>
            {
                AddBotMessage(response);
                _messageCount++;
                UpdateMessageCount();
                ScrollToBottom();
                SpeakText(response);
            });
        }

        private void HandleErrorOccurred(string errorMessage)
        {
            Dispatcher.Invoke(() =>
            {
                AddBotMessage("[ERROR] " + errorMessage);
                AddBotMessage("Please try again or rephrase your question.");
            });
        }

        private async void SendMessage()
        {
            string userMessage = UserInput.Text.Trim();
            if (string.IsNullOrEmpty(userMessage)) return;

            AddUserMessage(userMessage);
            _messageCount++;
            UpdateMessageCount();
            UserInput.Clear();

            SentimentResult sentiment = _sentimentAnalyzer.AnalyzeText(userMessage);
            UpdateSentimentDisplay(sentiment);
            UpdateCurrentTopic();

            await _chatbot.ProcessUserInputAsync(userMessage);
            UserInput.Focus();
        }

        private void AddUserMessage(string message)
        {
            var bubble = new Border { Style = (Style)FindResource("UserBubble"), HorizontalAlignment = HorizontalAlignment.Right };
            var stack = new StackPanel();
            var text = new TextBlock { Style = (Style)FindResource("UserText"), Text = message };
            var timestamp = new TextBlock { Style = (Style)FindResource("TimestampStyle"), Text = DateTime.Now.ToString("HH:mm"), HorizontalAlignment = HorizontalAlignment.Right };
            stack.Children.Add(text);
            stack.Children.Add(timestamp);
            bubble.Child = stack;
            ChatMessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        private void AddBotMessage(string message)
        {
            var bubble = new Border { Style = (Style)FindResource("BotBubble"), HorizontalAlignment = HorizontalAlignment.Left };
            var stack = new StackPanel();
            var text = new TextBlock { Style = (Style)FindResource("BotText"), Text = message };
            var timestamp = new TextBlock { Style = (Style)FindResource("TimestampStyle"), Text = DateTime.Now.ToString("HH:mm"), HorizontalAlignment = HorizontalAlignment.Left };
            stack.Children.Add(text);
            stack.Children.Add(timestamp);
            bubble.Child = stack;
            ChatMessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        private void UpdateSentimentDisplay(SentimentResult sentiment)
        {
            string moodText, icon;
            switch (sentiment.SentimentType)
            {
                case SentimentType.Positive: moodText = "Positive"; icon = ":-)"; SentimentLabel.Foreground = System.Windows.Media.Brushes.LightGreen; break;
                case SentimentType.Negative: moodText = "Concerned"; icon = ":-("; SentimentLabel.Foreground = System.Windows.Media.Brushes.Orange; break;
                case SentimentType.Angry: moodText = "Angry"; icon = ">:-("; SentimentLabel.Foreground = System.Windows.Media.Brushes.OrangeRed; break;
                case SentimentType.Frustrated: moodText = "Frustrated"; icon = ":-/"; SentimentLabel.Foreground = System.Windows.Media.Brushes.Orange; break;
                default: moodText = "Neutral"; icon = ":-|"; SentimentLabel.Foreground = System.Windows.Media.Brushes.LightBlue; break;
            }
            SentimentIcon.Text = icon;
            SentimentLabel.Text = "Mood: " + moodText;
        }

        private void UpdateCurrentTopic()
        {
            string topic = _chatbot.GetCurrentTopicFromMemory();
            if (!string.IsNullOrEmpty(topic))
            {
                CurrentTopicLabel.Text = topic.ToUpper();
                CurrentTopicLabel.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                CurrentTopicLabel.Text = "None";
                CurrentTopicLabel.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void UpdateMessageCount() => MessageCountLabel.Text = "Messages: " + _messageCount;
        private void UpdateMemoryStatus(bool active) => MemoryStatusLabel.Text = active ? "Active" : "Inactive";
        private void ScrollToBottom() => Dispatcher.Invoke(() => ChatScrollViewer.ScrollToBottom(), DispatcherPriority.Background);

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && !string.IsNullOrWhiteSpace(UserInput.Text))
                SendMessage();
        }
        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Clear all messages?", "Clear", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ChatMessagesPanel.Children.Clear();
                _messageCount = 0;
                UpdateMessageCount();
                _chatbot.ResetConversation();
                UpdateCurrentTopic();
                AddBotMessage("Chat cleared. How can I help?");
            }
        }

        private void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_speechRecognizer == null) { AddBotMessage("[VOICE] Voice not available. Please type."); return; }
            AddBotMessage("[VOICE] Listening... Speak now.");
            _speechRecognizer.RecognizeAsync(RecognizeMode.Single);
        }

        private void SessionButton_Click(object sender, RoutedEventArgs e)
        {
            var memory = _chatbot.GetUserMemory();
            var info = new StringBuilder();
            info.AppendLine("=== SESSION INFORMATION ===");
            info.AppendLine(_chatbot.GetSessionInfo());
            info.AppendLine("\nTopics Discussed:");
            if (memory.RecentTopics != null && memory.RecentTopics.Count > 0)
                foreach (var t in memory.RecentTopics) info.AppendLine(" - " + t);
            else info.AppendLine(" - None");
            info.AppendLine("\nConversation Flow:\n" + _chatbot.GetConversationFlow());
            MessageBox.Show(info.ToString(), "Session Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
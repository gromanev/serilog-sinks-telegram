using Serilog.Sinks.Telegram.Client;

namespace Serilog.Sinks.Telegram
{
    public sealed class TelegramMessage
    {
        public TelegramMessage(string text, TelegramParseModeTypes parseMode = TelegramParseModeTypes.Markdown)
        {
            Text = text;
            ParseMode = parseMode;
        }

        public string Text { get; }
        public TelegramParseModeTypes ParseMode { get; }
    }
}
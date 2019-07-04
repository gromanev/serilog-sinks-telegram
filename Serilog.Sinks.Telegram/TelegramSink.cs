using System;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Serilog.Sinks.Telegram
{
    public class TelegramSink : ILogEventSink
    {
        /// <summary>
        /// Delegate to allow overriding of the RenderMessage method.
        /// </summary>
        public delegate TelegramMessage RenderMessageMethod(LogEvent input);

        private readonly string _chatId;
        private readonly string _token;
        protected readonly IFormatProvider FormatProvider;
        protected readonly ITelegramClientFactory TelegramClientFactory;

        /// <summary>
        /// RenderMessage method that will transform LogEvent into a Telegram message.
        /// </summary>
        protected RenderMessageMethod RenderMessageImplementation = RenderMessage;

        public TelegramSink(string chatId, string token, 
            RenderMessageMethod renderMessageImplementation,
            IFormatProvider formatProvider,
            ITelegramClientFactory telegramClientFactory)
        {
            if (string.IsNullOrWhiteSpace(value: chatId))
                throw new ArgumentNullException(paramName: nameof(chatId));

            if (string.IsNullOrWhiteSpace(value: token))
                throw new ArgumentNullException(paramName: nameof(token));

            FormatProvider = formatProvider;
            if (renderMessageImplementation != null)
                RenderMessageImplementation = renderMessageImplementation;
            _chatId = chatId;
            _token = token;

            TelegramClientFactory = telegramClientFactory ?? new TelegramClientFactory();
        }

        #region ILogEventSink implementation

        public void Emit(LogEvent logEvent)
        {
            var message = FormatProvider != null
                ? new TelegramMessage(text: logEvent.RenderMessage(formatProvider: FormatProvider))
                : RenderMessageImplementation(input: logEvent);

            // The message renderer may decide to discard the message. If so, we skip sending
            // the current event and let things proceed. This is particularly useful for implementing
            // an 'intelligent' renderer and analyses the message and makes a decision to send
            // it to Telegram or not. Criteria such as message content could be used to make a
            // decision, or further messages to Telegram could be skipped if a threshold of 
            // "too many messages too fast" is passed, for example.
            if (message != null)
            {
                SendMessage(token: _token, chatId: _chatId, message: message);
            }
        }

        #endregion

        protected static TelegramMessage RenderMessage(LogEvent logEvent)
        {
            var sb = new StringBuilder();
            sb.AppendLine(value: $"{GetEmoji(log: logEvent)} {logEvent.RenderMessage()}");

            if (logEvent.Exception != null)
            {
                sb.AppendLine(value: $"\n*{logEvent.Exception.Message}*\n");
                sb.AppendLine(value: $"Message: `{logEvent.Exception.Message}`");
                sb.AppendLine(value: $"Type: `{logEvent.Exception.GetType().Name}`\n");
                sb.AppendLine(value: $"Stack Trace\n```{logEvent.Exception}```");
            }
            return new TelegramMessage(text: sb.ToString());
        }

        private static string GetEmoji(LogEvent log)
        {
            switch (log.Level)
            {
                case LogEventLevel.Debug:
                    return "👉";
                case LogEventLevel.Error:
                    return "❗";
                case LogEventLevel.Fatal:
                    return "‼";
                case LogEventLevel.Information:
                    return "ℹ";
                case LogEventLevel.Verbose:
                    return "⚡";
                case LogEventLevel.Warning:
                    return "⚠";
                default:
                    return "";
            }
        }

        protected void SendMessage(string token, string chatId, TelegramMessage message)
        {
            SelfLog.WriteLine($"Trying to send message to chatId '{chatId}': '{message}'.");

            ITelegramClient telegramClient = TelegramClientFactory.CreateClient(token);

            var sendMessageTask = telegramClient.PostAsync(message: message, chatId: chatId);
            Task.WaitAll(sendMessageTask);

            var sendMessageResult = sendMessageTask.Result;
            if (sendMessageResult != null)
                SelfLog.WriteLine($"Message sent to chatId '{chatId}': '{sendMessageResult.StatusCode}'.");
        }
    }
}
using System;
using System.Net;
using Serilog.Configuration;
using Serilog.Events;

namespace Serilog.Sinks.Telegram
{
    public static class TelegramSinkExtension
    {
        public static LoggerConfiguration Telegram(
            this LoggerSinkConfiguration loggerConfiguration,
            string token,
            string chatId,
            TelegramSink.RenderMessageMethod renderMessageImplementation = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider formatProvider = null,
            ITelegramClientFactory telegramClientFactory = null,
            IWebProxy proxy = null
        )
        {
            if (loggerConfiguration == null)
                throw new ArgumentNullException(nameof(loggerConfiguration));

            return loggerConfiguration.Sink(
                new TelegramSink(
                    chatId,
                    token,
                    renderMessageImplementation,
                    formatProvider,
                    telegramClientFactory,
                    proxy
                ),
                restrictedToMinimumLevel);
        }
    }
}

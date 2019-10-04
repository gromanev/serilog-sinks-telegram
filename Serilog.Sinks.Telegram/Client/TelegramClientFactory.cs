using System.Net;

namespace Serilog.Sinks.Telegram
{
    public class TelegramClientFactory : ITelegramClientFactory
    {
        public ITelegramClient CreateClient(string botToken, IWebProxy proxy, int timeoutSeconds = 10)
        {
            return new TelegramClient(botToken, proxy, timeoutSeconds);
        }
    }
}

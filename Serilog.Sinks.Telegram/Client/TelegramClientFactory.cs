using System.Net.Http;
using System.Threading.Tasks;

namespace Serilog.Sinks.Telegram
{
    public class TelegramClientFactory : ITelegramClientFactory
    {
        public ITelegramClient CreateClient(string botToken, int timeoutSeconds = 10)
        {
            return new TelegramClient(botToken, timeoutSeconds);
        }
    }
}
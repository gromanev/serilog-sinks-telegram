using System.Net.Http;
using System.Threading.Tasks;

namespace Serilog.Sinks.Telegram
{
    public interface ITelegramClientFactory
    {
        ITelegramClient CreateClient(string botToken, int timeoutSeconds = 10);
    }
}
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Serilog.Sinks.Telegram
{
    public interface ITelegramClientFactory
    {
        ITelegramClient CreateClient(string botToken, IWebProxy proxy, int timeoutSeconds = 10);
    }
}

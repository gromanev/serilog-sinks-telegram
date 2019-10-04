using System.Net.Http;
using System.Threading.Tasks;

namespace Serilog.Sinks.Telegram
{
    public interface ITelegramClient
    {
        Task<HttpResponseMessage> PostAsync(TelegramMessage message, string chatId);
    }
}
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Serilog.Sinks.Telegram
{
    public class TelegramClient
    {
        private readonly Uri _apiUrl;
        private readonly HttpClient _httpClient = new HttpClient();

        public TelegramClient(string botToken, int timeoutSeconds = 10)
        {
            if (string.IsNullOrEmpty(value: botToken))
                throw new ArgumentException(message: "Bot token can't be empty", paramName: nameof(botToken));

            _apiUrl = new Uri(uriString: $"https://api.telegram.org/bot{botToken}/sendMessage");
            _httpClient.Timeout = TimeSpan.FromSeconds(value: timeoutSeconds);
        }

        public async Task<HttpResponseMessage> PostAsync(TelegramMessage message, string chatId)
        {
            string json;

            // If the message type is 'Plain', the 'parse_mode' key CANNOT be present at all,
            // else the Telegram API will attempt to parse the message string as a MarkDown.
            if (message.ParseMode == Client.TelegramParseModeTypes.Plain)
            {
                var payload = new
                {
                    chat_id = chatId,
                    text = message.Text
                };
                json = JsonConvert.SerializeObject(value: payload);
            }
            else
            {
                var payload = new
                {
                    chat_id = chatId,
                    text = message.Text,
                    parse_mode = message.ParseMode.ToString().ToLower()
                };
                json = JsonConvert.SerializeObject(value: payload);
            }

            var response = await _httpClient.PostAsync(requestUri: _apiUrl,
                content: new StringContent(content: json, encoding: Encoding.UTF8, mediaType: "application/json"));

            return response;
        }
    }
}
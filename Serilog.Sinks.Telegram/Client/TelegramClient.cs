using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Serilog.Sinks.Telegram
{
    public class TelegramClient : ITelegramClient
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
            if (message.ParseMode == Client.TelegramParseModeTypes.Plain ||
                !ValidateMessage(message))
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

            var response = await _httpClient.PostAsync(
                requestUri: _apiUrl,
                content: new StringContent(
                    content: json, encoding: Encoding.UTF8, mediaType: "application/json"));

            return response;
        }

        /// <summary>
        /// These methods should be extended to validate all the cases where Telegram
        /// will reject a 'Markdown' or 'Html' formatted string as 'BadRequest'.
        /// </summary>
        /// <example>
        /// Message "*This is rejected" is rejected by Telegram as an invalid
        /// markdown because it has an odd number of asterisks, instead of being ignored 
        /// as it really should.
        /// </example>
        /// <param name="message"></param>
        /// <returns>
        /// True: message is valid for the parse mode type. 
        /// False: invalid and should probably be handled as normal text.
        /// </returns>
        private bool ValidateMessage(TelegramMessage message)
        {
            switch (message.ParseMode)
            {
                case Client.TelegramParseModeTypes.Html:
                    return ValidateMessageHtml(message);
                case Client.TelegramParseModeTypes.Markdown:
                    return ValidateMessageMarkdown(message);
                case Client.TelegramParseModeTypes.Plain:
                    break;
            }

            return true;    // Is valid
        }

        private bool ValidateMessageMarkdown(TelegramMessage message)
        {
            // Validate: *even is ok* and reject odd numbers of '*'
            if (!Regex.IsMatch(message.Text, @"^[^*]*(\*[^*]*\*[^*]*)*$")) return false;
            // Validate: _even is ok_ and reject odd numbers of '_'
            if (!Regex.IsMatch(message.Text, @"^[^_]*(_[^_]*_[^_]*)*$")) return false;

            // Validate: [inline URL](url)
            // Validate: [inline mention of a user](tg://user?id=123456789)

            // Validate: `inline fixed-width code`
            if (!Regex.IsMatch(message.Text, @"^[^(`)]*(`[^(`)]*`[^(`)]*)*$")) return false;

            // Validate:
            //      ```block_language
            //      pre - formatted fixed-width code block
            //      ```
            if (!Regex.IsMatch(message.Text, @"^[^(```)]*(```[^(```)]*```[^(```)]*)*$")) return false;

            return true;    // Assume valid if we passed all tests above.
        }

        private bool ValidateMessageHtml(TelegramMessage message)
        {
            // Validate the cases where Telegram returns 'BadRequest'. These cases
            // really need to be tested with real data because it's unclear what will
            // trigger an error from Telegram.

            return true;    // Assume everything is valid for now.
        }
    }
}
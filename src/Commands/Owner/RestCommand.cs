using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Configuration;

namespace OoLunar.Tomoe.Commands.Owner
{
    /// <summary>
    /// Mimimimi
    /// </summary>
    public sealed class RestCommand
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly TomoeConfiguration _configuration;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new instance of <see cref="RestCommand"/>.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="httpClient">The HTTP client to use.</param>
        public RestCommand(TomoeConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Executes an HTTP request to the specified URL. If the url is a Discord API endpoint, the Authorization header will be added.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="body">The optional json body to send with the request.</param>
        [Command("rest")]
        [RequireApplicationOwner]
        public async ValueTask ExecuteAsync(CommandContext context, string method, string url, [RemainingText] string? body = null)
        {
            HttpRequestMessage request = new(HttpMethod.Parse(method), url);
            request.Headers.Add("Accept", "application/json");
            if (body is not null)
            {
                request.Content = JsonContent.Create(JsonNode.Parse(body));
            }

            // Only add the Authorization header if the URL is a Discord API endpoint
            if (url.Contains("discord.com/api"))
            {
                request.Headers.TryAddWithoutValidation("Authorization", $"Bot {_configuration.Discord.Token}");
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            DiscordMessageBuilder messageBuilder = new();
            StringBuilder responseBuilder = new();
            responseBuilder.AppendLine($"Status Code: {(int)response.StatusCode} {response.StatusCode}");
            responseBuilder.AppendLine("Headers:");

            bool isJson = false;
            StringBuilder headersBuilder = new();
            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers.OrderBy(header => header.Key))
            {
                headersBuilder.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
                if (header.Key == "Content-Type" && header.Value.Any(value => value.Contains("application/json")))
                {
                    isJson = true;
                }
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Content.Headers.OrderBy(header => header.Key))
            {
                headersBuilder.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
                if (header.Key == "Content-Type" && header.Value.Any(value => value.Contains("application/json")))
                {
                    isJson = true;
                }
            }

            if ((headersBuilder.Length + responseBuilder.Length + 12) > 2000)
            {
                messageBuilder.AddFile("headers.txt", new MemoryStream(Encoding.UTF8.GetBytes(headersBuilder.ToString())), AddFileOptions.CloseStream);
            }
            else
            {
                responseBuilder.AppendLine("```");
                responseBuilder.AppendLine(headersBuilder.ToString());
                responseBuilder.AppendLine("```");
            }

            string content;
            if (isJson)
            {
                // Deserialize the JSON content to a JsonDocument and then serialize it back to a string to pretty print it
                content = JsonSerializer.Serialize(await JsonSerializer.DeserializeAsync<JsonDocument>(response.Content.ReadAsStream(), _jsonOptions), _jsonOptions);
            }
            else
            {
                // Read the content as a string
                content = await response.Content.ReadAsStringAsync();
            }

            responseBuilder.AppendLine("Content:");
            if ((content.Length + responseBuilder.Length + 12) > 2000)
            {
                messageBuilder.AddFile($"response.{(isJson ? "json" : "txt")}", new MemoryStream(Encoding.UTF8.GetBytes(content)), AddFileOptions.CloseStream);
            }
            else
            {
                responseBuilder.AppendLine("```json");
                responseBuilder.AppendLine(content);
                responseBuilder.AppendLine("```");
            }

            await context.RespondAsync(messageBuilder.WithContent(responseBuilder.ToString()));
        }
    }
}

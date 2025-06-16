using SmartSupport.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmartSupport.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        private InnerMessage ExtractInnerMessage(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            // Знаходимо позицію першої фігурної дужки — початок JSON
            int jsonStart = raw.IndexOf('{');
            if (jsonStart == -1)
                return null;

            string jsonPart = raw.Substring(jsonStart);

            try
            {
                var innerMessage = JsonSerializer.Deserialize<InnerMessage>(jsonPart, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return innerMessage;
            }
            catch
            {
                return null;
            }
        }

        public async Task<InnerMessage> GetAiResponseAsync(string question, string documentation, bool Islocal)
        {
            string apiKey = Islocal
                ? _configuration["DeepSeek:LocalApiKey"] ?? throw new ArgumentNullException("DeepSeek:LocalApiKey is missing")
                : _configuration["DeepSeek:ApiKey"] ?? throw new ArgumentNullException("DeepSeek:ApiKey is missing");

            string apiUrl = Islocal
                ? _configuration["DeepSeek:LocalApiUrl"] ?? throw new ArgumentNullException("DeepSeek:LocalApiUrl is missing")
                : _configuration["DeepSeek:ApiUrl"] ?? throw new ArgumentNullException("DeepSeek:ApiUrl is missing");

            object requestBody;

            if (Islocal)
            {
                requestBody = new
                {
                    query = question,
                    inputs = new { },
                    response_mode = "streaming",
                    auto_generate_name = true,
                    user = "artem-123"
                };
            }
            else
            {
                requestBody = new
                {
                    model = "deepseek-chat",
                    messages = new[]
                    {
                        new {
                            role = "system",
                            content = "You are acting as a technical support operator.\n\nYour Role:\nYou help customers by answering their questions using the documentation provided.\n\nWorkflow:\nYou will receive:\n- A message from a customer (in plain text).\n- A documentation text after the keyword: 'Documentation:'\n\nYour task is to:\n1. Understand the customer's question.\n2. Search the documentation to find relevant and accurate information.\n3. Reply to the customer in a clear, helpful, and friendly tone.\n4. Return the section title where the user can find more information.\n\n Response format:\nReturn your response strictly in the following JSON format:\n{\n  \"message\": \"[Answer to the question in a helpful and friendly tone.]\",\n  \"reference\": \"[Section Title from the documentation where the user can read more]\"\n}\n\n⚠️ Notes:\n- If the documentation doesn’t contain the answer, write a polite response suggesting the customer contact support.\n- Do not include any extra text outside the JSON structure.\n- Always make sure the JSON is valid."
                        },
                        new {
                            role = "user",
                            content = $"{question}, Documentation: {documentation}"
                        }
                    }
                };
            }

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"DeepSeek AI API error: {response.StatusCode}, {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            if (Islocal)
            {
                // Локальний API, який повертає streaming response (SSE)
                var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);

                var answerBuilder = new StringBuilder();

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:"))
                        continue;

                    var jsonLine = line["data:".Length..].Trim();
                    using var doc = JsonDocument.Parse(jsonLine);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("event", out var evt) &&
                        evt.GetString() == "agent_message" &&
                        root.TryGetProperty("answer", out var answer))
                    {
                        answerBuilder.Append(answer.GetString());
                    }
                }

                return ExtractInnerMessage(answerBuilder.ToString());

            }
            else
            {
                // Віддалений API - парсимо через Root та InnerMessage
                var root = JsonSerializer.Deserialize<Root>(responseJson);
                var responseContent = root!.choices.First().message.content;
                var cleanJson = Regex.Replace(responseContent, "```json|```", "").Trim();

                InnerMessage inner;
                try
                {
                    inner = JsonSerializer.Deserialize<InnerMessage>(cleanJson)!;
                }
                catch
                {
                    throw new InvalidOperationException("Failed to deserialize remote AI response");
                }
                return inner;
            }
        }

        public async Task<string> GetCategoriesAsync(string question, string documentation, bool Islocal)
        {
            string apiKey;
            string apiUrl;

            if (Islocal)
            {
                apiKey = _configuration["DeepSeek:LocalApiKey"]
                         ?? throw new ArgumentNullException("DeepSeek:LocalApiKey is missing");
                apiUrl = _configuration["DeepSeek:LocalApiUrl"]
                         ?? throw new ArgumentNullException("DeepSeek:LocalApiUrl is missing");
            }
            else
            {
                apiKey = _configuration["DeepSeek:ApiKey"]
                         ?? throw new ArgumentNullException("DeepSeek:ApiKey is missing");
                apiUrl = _configuration["DeepSeek:ApiUrl"]
                         ?? throw new ArgumentNullException("DeepSeek:ApiUrl is missing");
            }
            var requestBody = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
            new
            {
                role = "system",
                content = @"
                    You are acting as a technical support operator.

                    Your task is to:
                    1. Read the documentation.
                    2. Detect and list all available sections or categories at the beginning of your response, using the format:
                    Categories: [Category1], [Category2], [Category3], ...

                    3. Answer the user's question using the documentation in a friendly and helpful tone.

                    Notes:
                    - If documentation does not contain relevant information, inform the user and recommend contacting support."
            },
            new
            {
                role = "user",
                content = $"{question}\n\nDocumentation:\n{documentation}"
            }
        }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"DeepSeek AI API error: {response.StatusCode}, {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

    }


    public class Root
    {
        public List<Choice> choices { get; set; }
    }

    public class Choice
    {
        public AI_Message message { get; set; }
    }

    public class AI_Message
    {
        public string content { get; set; }
    }

    public class InnerMessage
    {
        public string message { get; set; }
        public string reference { get; set; }
    }

}

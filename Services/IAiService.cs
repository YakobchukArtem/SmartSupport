using SmartSupport.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmartSupport.Services
{
    public class AiService(HttpClient httpClient) : IAiService
    {
        private readonly string _apiKey = "sk-a3a877a7cd2a4b93a91711d171a02aef";
        private readonly string _apiUrl = "https://api.deepseek.com/chat/completions";

        public async Task<InnerMessage> GetAiResponseAsync(string question, string documentation)
        {
            var requestBody = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = "You are acting as a technical support operator.\n\nYour Role:\nYou help customers by answering their questions using the documentation provided.\n\nWorkflow:\nYou will receive:\n- A message from a customer (in plain text).\n- A documentation text after the keyword: 'Documentation:'\n\nYour task is to:\n1. Understand the customer's question.\n2. Search the documentation to find relevant and accurate information.\n3. Reply to the customer in a clear, helpful, and friendly tone.\n4. Return the section title where the user can find more information.\n\n Response format:\nReturn your response strictly in the following JSON format:\n{\n  \"message\": \"[Answer to the question in a helpful and friendly tone.]\",\n  \"reference\": \"[Section Title from the documentation where the user can read more]\"\n}\n\n⚠️ Notes:\n- If the documentation doesn’t contain the answer, write a polite response suggesting the customer contact support.\n- Do not include any extra text outside the JSON structure.\n- Always make sure the JSON is valid." },
                    new { role = "user", content = $"{question}, Documentation: {documentation}" }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await httpClient.PostAsync(_apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"DeepSeek AI API error: {response.StatusCode}, {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            var root = JsonSerializer.Deserialize<Root>(responseJson);

            var response_content = root.choices.First().message.content;

            var cleanJson = Regex.Replace(response_content, "```json|```", "").Trim();
            InnerMessage inner;
            try
            {
                inner = JsonSerializer.Deserialize<InnerMessage>(cleanJson);
            }
            catch
            {
                throw new InvalidOperationException("Failed deserialize response");
            }
            return inner;
        }

        public async Task<string> GetCategoriesAsync(string question, string documentation)
        {
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

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await httpClient.PostAsync(_apiUrl, content);

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

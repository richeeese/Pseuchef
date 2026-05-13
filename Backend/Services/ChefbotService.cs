using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Pseuchef.Services
{
    public class ChefbotService
    {
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        private static readonly HttpClient client = new HttpClient();

        private List<object> conversationHistory = new List<object>();

        private readonly string systemContext = @"You are Chefbot, a friendly and knowledgeable AI culinary assistant built into Pseuchef — an intelligent cooking companion app. 
            You help users with:
            - Recipe recommendations and ideas
    - Step-by-step cooking guidance
    - Ingredient substitutions
    - Cooking tips and techniques
    - Dietary advice and meal planning
    Keep responses concise, warm, and practical. You're like a helpful friend in the kitchen.";

        public async Task<string> SendMessageAsync(string userMessage)
        {
            try
            {
                conversationHistory.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = userMessage } }
                });

                string url = $"{BaseUrl}?key={Config.GeminiApiKey}";

                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = systemContext } }
                    },
                    contents = conversationHistory
                };

                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                string responseJson = await response.Content.ReadAsStringAsync();

                var data = JObject.Parse(responseJson);

                if (data["error"] != null)
                {
                    return $"API Error: {data["error"]["message"]}";
                }

                string reply = data["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString()
                               ?? $"Unknown Error. Raw response: {responseJson}";

                conversationHistory.Add(new
                {
                    role = "model",
                    parts = new[] { new { text = reply } }
                });

                return reply;
            }
            catch (Exception ex)
            {
                return $"Chefbot is unavailable right now: {ex.Message}";
            }
        }

        public void ClearHistory()
        {
            conversationHistory.Clear();
        }
    }
}
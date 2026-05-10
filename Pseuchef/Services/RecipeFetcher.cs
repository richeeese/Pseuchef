using Newtonsoft.Json.Linq;
using Pseuchef.Interfaces;
using Pseuchef.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pseuchef.Services
{
    internal class RecipeFetcher : IRecipeService
    {
        private const string BaseUrl = "https://api.spoonacular.com";
        private static readonly HttpClient client = new HttpClient();

        public List<Recipe> Search(List<string> inventory, UserProfile profile)
        {
            return SearchAsync(inventory, profile).GetAwaiter().GetResult();
        }

        private async Task<List<Recipe>> SearchAsync(List<string> inventory, UserProfile profile)
        {
            if (inventory == null || inventory.Count == 0)
                return new List<Recipe>();

            string ingredients = string.Join(",", inventory);
            string diet = profile?.GetAPIFilterString() ?? "";

            string url = $"{BaseUrl}/recipes/findByIngredients" +
                         $"?ingredients={Uri.EscapeDataString(ingredients)}" +
                         $"&number=5" +
                         $"&apiKey={Config.SpoonacularApiKey}";

            if (!string.IsNullOrEmpty(diet))
                url += $"&diet={diet}";

            string json = await client.GetStringAsync(url);
            var data = JArray.Parse(json);
            var results = new List<Recipe>();

            foreach (var item in data)
            {
                string title = item["title"]?.ToString() ?? "Unknown";
                string imageUrl = item["image"]?.ToString() ?? "";
                int used = item["usedIngredientCount"]?.Value<int>() ?? 0;
                int missed = item["missedIngredientCount"]?.Value<int>() ?? 0;

                var ingredientList = new List<RecipeIngredient>();

                var usedIngredients = item["usedIngredients"] as JArray;
                if (usedIngredients != null)
                {
                    foreach (var ing in usedIngredients)
                    {
                        string name = ing["name"]?.ToString() ?? "Unknown";
                        ingredientList.Add(new RecipeIngredient(name, usedCount: 1, missedCount: 0));
                    }
                }

                var missedIngredients = item["missedIngredients"] as JArray;
                if (missedIngredients != null)
                {
                    foreach (var ing in missedIngredients)
                    {
                        string name = ing["name"]?.ToString() ?? "Unknown";
                        ingredientList.Add(new RecipeIngredient(name, usedCount: 0, missedCount: 1));
                    }
                }

                results.Add(new Recipe(title, ingredientList, 0, imageUrl));
            }

            return results;
        }
    }
}
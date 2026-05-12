using System.Net.Http;
using System.Text.Json;
using Pseuchef.Models;
using Pseuchef.Interfaces;

namespace Pseuchef.Services
{
    public class RecipeFetcher : IRecipeService
    {
        private static readonly HttpClient _http = new HttpClient();

        public List<Recipe> Search(List<string> inventory, UserProfile profile)
        {
            if (inventory == null || inventory.Count == 0)
                return new List<Recipe>();

            string ingredients = string.Join(",+", inventory);
            string searchUrl = "https://api.spoonacular.com/recipes/findByIngredients"
                             + $"?ingredients={Uri.EscapeDataString(ingredients)}"
                             + "&number=20"
                             + "&ranking=2"
                             + "&ignorePantry=true"
                             + $"&apiKey={Config.SpoonacularApiKey}";

            try
            {
                // ── Step 1: Get recipes + ingredient match data ──────────────────
                var searchResponse = Task.Run(() => _http.GetStringAsync(searchUrl)).Result;
                var searchJson = JsonDocument.Parse(searchResponse);

                var recipes = new List<Recipe>();
                var idToData = new Dictionary<int, (List<RecipeIngredient> ingredients, int matchCount)>();

                foreach (var item in searchJson.RootElement.EnumerateArray())
                {
                    var ingredientList = new List<RecipeIngredient>();

                    foreach (var ing in item.GetProperty("usedIngredients").EnumerateArray())
                    {
                        string name = ing.GetProperty("name").GetString() ?? "";
                        if (!string.IsNullOrWhiteSpace(name))
                            ingredientList.Add(new RecipeIngredient(name, usedCount: 1, missedCount: 0));
                    }

                    foreach (var ing in item.GetProperty("missedIngredients").EnumerateArray())
                    {
                        string name = ing.GetProperty("name").GetString() ?? "";
                        if (!string.IsNullOrWhiteSpace(name))
                            ingredientList.Add(new RecipeIngredient(name, usedCount: 0, missedCount: 1));
                    }

                    int matchCount = ingredientList.Count(i => i.usedCount > 0);
                    if (matchCount < 1) continue; // skip zero-match recipes

                    int id = item.GetProperty("id").GetInt32();
                    idToData[id] = (ingredientList, matchCount);
                }

                if (idToData.Count == 0) return new List<Recipe>();

                // ── Step 2: Bulk fetch prep time + servings for all recipe IDs ───
                string ids = string.Join(",", idToData.Keys);
                string bulkUrl = "https://api.spoonacular.com/recipes/informationBulk"
                                + $"?ids={ids}"
                                + $"&apiKey={Config.SpoonacularApiKey}";

                var bulkResponse = Task.Run(() => _http.GetStringAsync(bulkUrl)).Result;
                var bulkJson = JsonDocument.Parse(bulkResponse);

                foreach (var item in bulkJson.RootElement.EnumerateArray())
                {
                    int id = item.GetProperty("id").GetInt32();
                    string title = item.GetProperty("title").GetString() ?? "Unknown Recipe";
                    double prepTime = item.TryGetProperty("readyInMinutes", out var t) ? t.GetDouble() : 0;
                    string imageUrl = item.TryGetProperty("image", out var img) ? img.GetString() ?? "" : "";
                    int servings = item.TryGetProperty("servings", out var s) ? s.GetInt32() : 0;

                    if (!idToData.TryGetValue(id, out var data)) continue;

                    recipes.Add(new Recipe(title, data.ingredients, prepTime, imageUrl, servings, id));
                }

                return recipes.Take(3).ToList();
            }
            catch
            {
                return new List<Recipe>();
            }
        }

        /// <summary>
        /// Fetches full recipe instructions for a given Spoonacular recipe ID.
        /// Called when the user clicks Cook Now on a recipe card.
        /// Returns a list of step strings.
        /// </summary>
        public List<string> GetSteps(int recipeId)
        {
            string url = $"https://api.spoonacular.com/recipes/{recipeId}/analyzedInstructions"
                       + $"?apiKey={Config.SpoonacularApiKey}";

            try
            {
                var response = Task.Run(() => _http.GetStringAsync(url)).Result;
                var json = JsonDocument.Parse(response);

                var steps = new List<string>();

                foreach (var section in json.RootElement.EnumerateArray())
                {
                    foreach (var step in section.GetProperty("steps").EnumerateArray())
                    {
                        string text = step.GetProperty("step").GetString() ?? "";
                        if (!string.IsNullOrWhiteSpace(text))
                            steps.Add(text);
                    }
                }

                if (steps.Count == 0)
                    return new List<string>
                {
                    "This recipe doesn't have step-by-step instructions on Spoonacular.",
                    $"Search \"{recipeId}\" on spoonacular.com for the full recipe."
                };

                return steps;
            }
            catch
            {
                return new List<string>
                {
                    "Could not load instructions — check your connection or API key."
                };
            }
        }
    }
}
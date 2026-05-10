using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pseuchef.Models
{
    public class Recipe
    {
        public string RecipeTitle { get; private set; }
        public List<string> IngredientsNeeded { get; private set; }
        public double PrepTime { get; private set; }

        public Recipe(string title, List<string> ingredients, double time)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Recipe title cannot be empty", nameof(title));
            if (time < 0)
                throw new ArgumentException("Prep time cannot be negative", nameof(time));

            RecipeTitle = title;
            IngredientsNeeded = ingredients ?? new List<string>();
            PrepTime = time;
        }

        public string GetTitle() => RecipeTitle;

        public IReadOnlyList<string> GetIngredients() => IngredientsNeeded.AsReadOnly();

        public double GetPrepTime() => PrepTime;

        public int GetIngredientCount() => IngredientsNeeded.Count;

        public bool ContainsIngredient(string ingredient)
        {
            if (string.IsNullOrWhiteSpace(ingredient))
                return false;

            return IngredientsNeeded.Any(ing => 
                ing.Equals(ingredient, StringComparison.OrdinalIgnoreCase));
        }

        public string DisplayRecipe()
        {
            var recipeDisplay = new StringBuilder();
            recipeDisplay.AppendLine("========================================");
            recipeDisplay.AppendLine($"Recipe: {RecipeTitle}");
            recipeDisplay.AppendLine("========================================");
            recipeDisplay.AppendLine($"Preparation Time: {PrepTime} minutes");
            recipeDisplay.AppendLine($"Number of Ingredients: {GetIngredientCount()}");

            if (IngredientsNeeded.Count > 0)
            {
                recipeDisplay.AppendLine();
                recipeDisplay.AppendLine("Ingredients:");
                for (int i = 0; i < IngredientsNeeded.Count; i++)
                {
                    recipeDisplay.AppendLine($"  {i + 1}. {IngredientsNeeded[i]}");
                }
            }
            else
            {
                recipeDisplay.AppendLine("Ingredients: None");
            }

            recipeDisplay.AppendLine("========================================");

            return recipeDisplay.ToString();
        }

        public void PrintRecipe()
        {
            Console.WriteLine(DisplayRecipe());
        }

        public override string ToString()
        {
            return $"{RecipeTitle} ({GetIngredientCount()} ingredients, {PrepTime} min)";
        }
    }
}

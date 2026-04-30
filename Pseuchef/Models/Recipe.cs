using System;
using System.Collections.Generic;
using System.Text;
using Pseuchef.Services;

namespace Pseuchef.Models
{
    internal class Recipe
    {
        private string recipeTitle { get; set; };
        private List<string> ingredientsNeeded { get; set; }>;
        private double prepTime { get; set; };

        Recipe(string title, List<string> ingredients, double time)
        {
            //auto-fill
            recipeTitle = title;
            ingredientsNeeded = ingredients;
            prepTime = time;
        }

        private void displayRecipe()
        {

        }
    }
}

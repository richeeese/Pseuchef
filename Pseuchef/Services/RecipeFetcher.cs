using System;
using System.Collections.Generic;
using System.Text;

namespace Pseuchef.Services
{
    internal class RecipeFetcher : IRecipeService
    {
        public List<Recipes> getRecipes(List<string> ingredients);
    }
}

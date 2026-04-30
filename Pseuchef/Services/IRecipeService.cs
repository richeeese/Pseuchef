using System;
using System.Collections.Generic;
using System.Text;

namespace Pseuchef.Services
{
    internal interface IRecipeService
    {
        public List<Recipes> getRecipes(List<string> ingredients);
    }
}

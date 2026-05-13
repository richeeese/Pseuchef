using System;
using System.Collections.Generic;
using Pseuchef.Models;

namespace Pseuchef.Interfaces
{
    // filtering recipes based on certain cr5iteria
    public interface IRecipeFilterStrategy
    {
        public List<Recipe> Filter(List<Recipe> recipes);

        string GetDescription();
    }
}

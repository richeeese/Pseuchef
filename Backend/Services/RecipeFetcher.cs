using System;
using System.Collections.Generic;
using System.Text;
using Pseuchef.Interfaces;
using Pseuchef.Models;

namespace Pseuchef.Services
{
    public class RecipeFetcher : IRecipeService
    {
        public List<Recipe> Search(List<string> inventory, UserProfile profile)
        {
            if (inventory == null) inventory = new List<string>();
            if (profile == null) return new List<Recipe>();

            // TODO: implement actual search logic using inventory and profile.
            return new List<Recipe>();
        }
    }
}

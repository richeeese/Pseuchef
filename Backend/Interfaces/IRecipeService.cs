using System;
using System.Collections.Generic;
using System.Text;
using Pseuchef.Models;

namespace Pseuchef.Interfaces
{
    internal interface IRecipeService
    {
        public List<Recipe> Search(List<string> inventory, UserProfile profile);
    }
}

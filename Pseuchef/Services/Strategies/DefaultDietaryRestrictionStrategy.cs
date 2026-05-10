using System;
using System.Collections.Generic;
using Pseuchef.Enums;
using Pseuchef.Interfaces;

namespace Pseuchef.Services.Strategies
{
    public class DefaultDietaryRestrictionStrategy : IDietaryRestrictionStrategy
    {
        public List<string> GetExcludedIngredients(DietaryRestriction restriction)
        {
            return restriction switch
            {
                DietaryRestriction.Vegan => new List<string> 
                { 
                    "Meat", "Poultry", "Fish", "Seafood", "Dairy", "Eggs", "Honey", "Gelatin" 
                },

                DietaryRestriction.Vegetarian => new List<string> 
                { 
                    "Meat", "Poultry", "Fish", "Seafood" 
                },

                DietaryRestriction.GlutenFree => new List<string> 
                { 
                    "Wheat", "Barley", "Rye", "Oats", "Bread", "Pasta", "Cereals" 
                },

                DietaryRestriction.DairyFree => new List<string> 
                { 
                    "Milk", "Cheese", "Cream", "Butter", "Yogurt", "Ice Cream" 
                },

                DietaryRestriction.None => new List<string>(),

                _ => new List<string>()
            };
        }

        public string GetDescription(DietaryRestriction restriction)
        {
            return restriction switch
            {
                DietaryRestriction.Vegan => "Vegan - No animal products",
                DietaryRestriction.Vegetarian => "Vegetarian - No meat",
                DietaryRestriction.GlutenFree => "Gluten Free - No gluten products",
                DietaryRestriction.DairyFree => "Dairy Free - No dairy products",
                DietaryRestriction.None => "No restrictions",
                _ => "Unknown restriction"
            };
        }
    }
}

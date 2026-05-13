using System;
using System.Collections.Generic;
using System.Linq;
using Pseuchef.Models;
using Pseuchef.Interfaces;
using Pseuchef.Enums;

namespace Pseuchef.Services.Strategies
{

    public class CategoryFilterStrategy : IFoodItemFilterStrategy
    {
        private FoodCategory _category;

        public CategoryFilterStrategy(FoodCategory category)
        {
            _category = category;
        }

        public List<FoodItem> Filter(List<FoodItem> items)
        {
            if (items == null)
                return new List<FoodItem>();

            return items.Where(item => item.category == _category).ToList();
        }

        public string GetDescription()
        {
            return $"Items in category: {_category}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Pseuchef.Models;
using Pseuchef.Interfaces;

namespace Pseuchef.Services.Strategies
{
    public class ExpiringItemFilterStrategy : IFoodItemFilterStrategy
    {
        private int _daysUntilExpiry;

        public ExpiringItemFilterStrategy(int daysUntilExpiry = 3)
        {
            _daysUntilExpiry = daysUntilExpiry;
        }

        public List<FoodItem> Filter(List<FoodItem> items)
        {
            if (items == null)
                return new List<FoodItem>();

            return items.OfType<PerishableItem>()
                .Where(item => item.GetDaysRemaining() <= _daysUntilExpiry)
                .Cast<FoodItem>()
                .ToList();
        }

        public string GetDescription()
        {
            return $"Items expiring within {_daysUntilExpiry} days";
        }
    }
}

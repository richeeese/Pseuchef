using System;
using System.Collections.Generic;
using System.Linq;
using Pseuchef.Models;
using Pseuchef.Interfaces;

namespace Pseuchef.Services.Strategies
{
    public class SortByExpiryStrategy : IInventorySortStrategy
    {
        public List<FoodItem> Sort(List<FoodItem> items)
        {
            if (items == null)
                return new List<FoodItem>();

            return items.OrderBy(item =>
            {
                if (item is PerishableItem perishable)
                    return perishable.GetExpiryDate();
                return DateOnly.MaxValue; // Non-perishable items go to the end
            }).ToList();
        }

        public string GetDescription()
        {
            return "Sorted by expiry date (soonest first)";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Pseuchef.Models
{
    internal class PerishableItem: FoodItem
    {
        private DateOnly expiryDate;

        PerishableItem(string itemName, FoodCategory category, bool isCooked, bool isFrozen, double calorieCount, DateOnly expiryDate): base(itemName, category, isCooked, isFrozen, calorieCount)
        {
            this.expiryDate = expiryDate;
        }

        public void expiryAlert()
        {
        
        }

        public DateOnly getExpiryDate()
        {
            return this.expiryDate;
        }

        public void setExpiryDate(DateOnly expiryDate)
        {
            this.expiryDate = expiryDate;
        }
    }
}

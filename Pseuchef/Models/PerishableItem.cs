using System;
using System.Collections.Generic;
using System.Text;
using Pseuchef.Enums;

namespace Pseuchef.Models
{
    internal class PerishableItem : FoodItem
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

        public bool isExpired()
        {
            return this.expiryDate < DateOnly.FromDateTime(DateTime.Now);
        }

        public int getDaysRemaining()
        {
            return (this.expiryDate.ToDateTime(TimeOnly.MinValue) - DateTime.Now).Days;
        }

        public override string ToString()
        {
            return base.ToString() + $", ExpiryDate: {expiryDate}";
        }
    }
}

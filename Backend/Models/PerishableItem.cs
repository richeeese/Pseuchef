using System;
using System.Collections.Generic;
using System.Text;
using Pseuchef.Enums;

namespace Pseuchef.Models
{
    public class PerishableItem : FoodItem
    {
        private DateOnly expiryDate;

        public PerishableItem(string itemName, FoodCategory category, bool isCooked, bool isFrozen, double calorieCount, DateOnly expiryDate): base(itemName, category, isCooked, isFrozen, calorieCount)
        {
            this.expiryDate = expiryDate;
        }

        public void ExpiryAlert()
        {
        
        }

        public DateOnly GetExpiryDate()
        {
            return this.expiryDate;
        }

        public void SetExpiryDate(DateOnly expiryDate)
        {
            this.expiryDate = expiryDate;
        }

        public bool IsExpired()
        {
            return this.expiryDate < DateOnly.FromDateTime(DateTime.Now);
        }

        public int GetDaysRemaining()
        {
            return (this.expiryDate.ToDateTime(TimeOnly.MinValue) - DateTime.Now).Days;
        }

        public override string ToString()
        {
            return base.ToString() + $", ExpiryDate: {expiryDate}";
        }
    }
}

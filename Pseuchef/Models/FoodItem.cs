using System;
using System.Collections.Generic;
using System.Text;
using Pseuchef.Enums;

namespace Pseuchef.Models
{
    internal class FoodItem
    {
        private string itemName;
        private FoodCategory category;
        private bool isCooked;
        private bool isFrozen;
        private double calorieCount;

        FoodItem(string itemName, FoodCategory category, bool isCooked, bool isFrozen, double calorieCount)
        {
            this.itemName = itemName;
            this.category = category;
            this.isCooked = isCooked;
            this.isFrozen = isFrozen;
            this.calorieCount = calorieCount;
        }

        public double getCalories()
        {
            return this.calorieCount;
        }

        public FoodCategory getCategory()
        {
            return this.category;
        }

        public void setCalories(double calories)
        {
            this.calorieCount = calories;
        }

        public void setCategory(FoodCategory category)
        {
            this.category = category;
        }
    }
}

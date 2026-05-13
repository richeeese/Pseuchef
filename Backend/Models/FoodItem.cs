using System;
using System.Collections.Generic;
using System.Text;
using Pseuchef.Enums;

namespace Pseuchef.Models
{
    public class FoodItem
    {
        public string itemName { get; private set; }
        public FoodCategory category { get; private set; }
        public bool isCooked { get; private set; }
        public bool isFrozen { get; private set; }
        public double calorieCount { get; private set; } = 0;

        public FoodItem(string itemName, FoodCategory category, bool isCooked, bool isFrozen, double calorieCount)
        {
            this.itemName = itemName;
            this.category = category;
            this.isCooked = isCooked;
            this.isFrozen = isFrozen;
            this.calorieCount = calorieCount;
        }

        public override string ToString()
        {
            return $"FoodItem: {itemName}, Category: {category}, IsCooked: {isCooked}, IsFrozen: {isFrozen}, CalorieCount: {calorieCount}";
        }

        public void UpdateItem(string itemName, FoodCategory category, bool isCooked, bool isFrozen, double calorieCount)
        {
            this.itemName = itemName;
            this.category = category;
            this.isCooked = isCooked;
            this.isFrozen = isFrozen;
            this.calorieCount = calorieCount;
        }

        public bool IsCompatibleWith(DietaryRestriction restriction)
        {
            switch (restriction)
            {
                case DietaryRestriction.None:
                    return true;
                case DietaryRestriction.Vegan:
                    return category != FoodCategory.Meat && category != FoodCategory.Dairy && category != FoodCategory.Poultry;
                case DietaryRestriction.Vegetarian:
                    return category != FoodCategory.Meat;
                case DietaryRestriction.Pescatarian:
                    return category != FoodCategory.Meat || category == FoodCategory.Fish;
                case DietaryRestriction.GlutenFree:
                    return category != FoodCategory.Grains;
                case DietaryRestriction.DairyFree:
                    return category != FoodCategory.Dairy;
                case DietaryRestriction.NutFree:
                    return category != FoodCategory.Nuts;
                case DietaryRestriction.LowCarb:
                    return category != FoodCategory.Grains && category != FoodCategory.Sugars;
                case DietaryRestriction.Keto:
                    return category == FoodCategory.Meat || category == FoodCategory.Fish || category == FoodCategory.Poultry || category == FoodCategory.Dairy || category == FoodCategory.Vegetables || category == FoodCategory.FatsAndOils;
                default:
                    throw new ArgumentOutOfRangeException(nameof(restriction), restriction, null);
            }
        }

        public FoodItem Clone()
        {
            return new FoodItem(itemName, category, isCooked, isFrozen, calorieCount);
        }
    }
}
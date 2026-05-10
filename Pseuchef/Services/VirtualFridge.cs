using System;
using System.Collections.Generic;
using System.Linq;
using Pseuchef.Models;
using Pseuchef.Interfaces;
using Pseuchef.Services.Strategies;

namespace Pseuchef.Services
{
    public class VirtualFridge
    {
        private List<FoodItem> inventory;

        public VirtualFridge()
        {
            inventory = new List<FoodItem>();
        }

        public void AddItem(FoodItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            inventory.Add(item);
        }

        public void RemoveItem(FoodItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            inventory.Remove(item);
        }

        public void UpdateItem(FoodItem oldItem, FoodItem newItem)
        {
            if (oldItem == null)
                throw new ArgumentNullException(nameof(oldItem), "Old item cannot be null");
            if (newItem == null)
                throw new ArgumentNullException(nameof(newItem), "New item cannot be null");

            int index = inventory.IndexOf(oldItem);
            if (index >= 0)
                inventory[index] = newItem;
        }

        public IReadOnlyList<FoodItem> GetInventory()
        {
            return inventory.AsReadOnly();
        }

        public int GetItemCount()
        {
            return inventory.Count;
        }

        public bool IsEmpty()
        {
            return inventory.Count == 0;
        }

        public void ClearInventory()
        {
            inventory.Clear();
        }

        public List<FoodItem> FilterItems(IFoodItemFilterStrategy filterStrategy)
        {
            if (filterStrategy == null)
                throw new ArgumentNullException(nameof(filterStrategy), "Filter strategy cannot be null");

            return filterStrategy.Filter(inventory);
        }

        public List<FoodItem> SortItems(IInventorySortStrategy sortStrategy)
        {
            if (sortStrategy == null)
                throw new ArgumentNullException(nameof(sortStrategy), "Sort strategy cannot be null");

            return sortStrategy.Sort(inventory);
        }

        public double CalculateMetric(ICalculationStrategy calculationStrategy)
        {
            if (calculationStrategy == null)
                throw new ArgumentNullException(nameof(calculationStrategy), "Calculation strategy cannot be null");

            return calculationStrategy.Calculate(inventory);
        }

        public List<FoodItem> GetExpiringItems(int daysUntilExpiry = 3)
        {
            if (daysUntilExpiry < 0)
                throw new ArgumentException("Days until expiry cannot be negative", nameof(daysUntilExpiry));

            var expiringStrategy = new ExpiringItemFilterStrategy(daysUntilExpiry);
            return FilterItems(expiringStrategy);
        }

        public List<FoodItem> GetItemsByCategory(Enums.FoodCategory category)
        {
            var categoryStrategy = new CategoryFilterStrategy(category);
            return FilterItems(categoryStrategy);
        }

        public List<FoodItem> GetItemsSortedByExpiry()
        {
            var expiryStrategy = new SortByExpiryStrategy();
            return SortItems(expiryStrategy);
        }

        public double CalculateTotalCalories()
        {
            var calorieStrategy = new CalorieCalculationStrategy();
            return CalculateMetric(calorieStrategy);
        }

        public List<FoodItem> FindExpiredItems()
        {
            return inventory.OfType<PerishableItem>()
                .Where(item => item.IsExpired())
                .Cast<FoodItem>()
                .ToList();
        }

        public int RemoveExpiredItems()
        {
            var expiredItems = FindExpiredItems();
            foreach (var item in expiredItems)
            {
                RemoveItem(item);
            }
            return expiredItems.Count;
        }
    }
}

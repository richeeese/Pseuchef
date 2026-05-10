using System;
using System.Collections.Generic;
using System.Text;
using Pseuchef.Models;

namespace Pseuchef.Services
{
    internal class VirtualFridge
    {
        private List<FoodItem> inventory;

        VirtualFridge()
        {
            inventory = new List<FoodItem>();
        }

        public void addItem(FoodItem item)
        {
            inventory.Add(item);
        }

        public void removeItem(FoodItem item)
        {
            inventory.Remove(item);
        }

        public List<FoodItem> getExpiring(int index)
        {
            // This method should return a list of items that are expiring soon.
            // For now, it returns an empty list as a placeholder.
            return new List<FoodItem>();
        }

        public double calculateCalories(List<FoodItem>)
        {
            // i don't think ito yung pinaka-code mismo, for now nag-autofill muna ako - ritzy
            return inventory.Count;
        }
    }
}

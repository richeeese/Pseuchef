using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pseuchef.Models;

namespace Pseuchef.Services
{
    internal class ShoppingListManager
    {
        /// <summary>
        /// Generates a shopping list by identifying recipe ingredients that are not available in the virtual fridge.
        /// Uses case-insensitive name matching to compare recipe ingredients with fridge inventory.
        /// </summary>
        /// <param name="recipe">The recipe to generate shopping list for</param>
        /// <param name="vfridge">The virtual fridge containing available inventory</param>
        /// <returns>A list of FoodItems that are needed but not in the fridge</returns>
        public List<FoodItem> GenerateList(Recipe recipe, VirtualFridge vfridge)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe), "Recipe cannot be null");
            if (vfridge == null)
                throw new ArgumentNullException(nameof(vfridge), "Virtual fridge cannot be null");

            var shoppingList = new List<FoodItem>();
            var recipeIngredients = recipe.GetIngredients();
            var fridgeInventory = vfridge.GetInventory();

            // Get all item names currently in the fridge (case-insensitive)
            var fridgeItemNames = fridgeInventory
                .Select(item => item.itemName.ToLower())
                .ToHashSet();

            // Check each recipe ingredient against fridge inventory
            foreach (var ingredient in recipeIngredients)
            {
                string ingredientLower = ingredient.ingredientName.ToLower();

                // If ingredient is not in fridge, add it to shopping list
                if (!fridgeItemNames.Contains(ingredientLower))
                {
                    // Try to find a matching FoodItem in fridge by name match
                    var matchingItem = FindItemByName(ingredient.ingredientName, fridgeInventory);

                    if (matchingItem != null)
                    {
                        // If we found a similar item, clone it (in case user wants to create a new one)
                        shoppingList.Add(matchingItem.Clone());
                    }
                    else
                    {
                        // Create a placeholder FoodItem for the missing ingredient
                        // Category defaults to Produce if not determinable
                        var placeholderItem = new FoodItem(
                            ingredient.ingredientName,
                            Enums.FoodCategory.Produce,
                            false,
                            false,
                            0
                        );
                        shoppingList.Add(placeholderItem);
                    }
                }
            }

            return shoppingList;
        }

        /// <summary>
        /// Finds a FoodItem in the inventory by name using case-insensitive matching.
        /// Returns null if no exact match is found.
        /// </summary>
        /// <param name="itemName">The name of the item to find</param>
        /// <param name="inventory">The inventory to search in</param>
        /// <returns>The matching FoodItem or null if not found</returns>
        private FoodItem FindItemByName(string itemName, IReadOnlyList<FoodItem> inventory)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return null;

            string searchName = itemName.ToLower();

            return inventory.FirstOrDefault(item =>
                item.itemName.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Exports a shopping list to a text file.
        /// </summary>
        /// <param name="list">The shopping list to export</param>
        /// <param name="filePath">The file path where the shopping list should be saved</param>
        public void ExportToText(List<FoodItem> list, string filePath)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list), "Shopping list cannot be null");
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("========================================");
                sb.AppendLine("SHOPPING LIST");
                sb.AppendLine("========================================");
                sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Items: {list.Count}");
                sb.AppendLine();

                if (list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        sb.AppendLine($"{i + 1}. {list[i].itemName}");
                        sb.AppendLine($"   Category: {list[i].category}");
                    }
                }
                else
                {
                    sb.AppendLine("No items needed - all recipe ingredients are in stock!");
                }

                sb.AppendLine();
                sb.AppendLine("========================================");

                System.IO.File.WriteAllText(filePath, sb.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to export shopping list to {filePath}", ex);
            }
        }
    }
}

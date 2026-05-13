// USAGE EXAMPLE: Recipe Class with Dish Image and Ingredient Tracking
//
// This document shows how to use the enhanced Recipe class with:
// - Single dish image (imageUrl) for the entire recipe
// - Ingredient tracking (usedCount, missedCount)

using System;
using System.Collections.Generic;
using Pseuchef.Models;
using Pseuchef.Services;

// ============================================================================
// EXAMPLE 1: Creating a Recipe with Ingredients and Dish Image
// ============================================================================

// Create individual recipe ingredients with tracking
var tomatoIngredient = new RecipeIngredient(
    ingredientName: "Tomato",
    usedCount: 5,
    missedCount: 2
);

var basilIngredient = new RecipeIngredient(
    ingredientName: "Basil",
    usedCount: 8,
    missedCount: 0
);

var mozzarellaIngredient = new RecipeIngredient(
    ingredientName: "Mozzarella Cheese",
    usedCount: 3,
    missedCount: 1
);

// Create the recipe with the ingredient list and a single dish image
var ingredients = new List<RecipeIngredient> 
{ 
    tomatoIngredient, 
    basilIngredient, 
    mozzarellaIngredient 
};

var margheritaRecipe = new Recipe(
    title: "Margherita Pizza",
    ingredients: ingredients,
    time: 30,
    imageUrl: "https://example.com/images/margherita-pizza.jpg"
);

// ============================================================================
// EXAMPLE 2: Displaying Recipe with Dish Image
// ============================================================================

// Display the recipe (now includes dish image)
Console.WriteLine(margheritaRecipe.DisplayRecipe());

// Output will show:
// ========================================
// Recipe: Margherita Pizza
// ========================================
// Image: https://example.com/images/margherita-pizza.jpg
// Preparation Time: 30 minutes
// Number of Ingredients: 3
//
// Ingredients:
//   1. Tomato
//      Used: 5, Missed: 2
//   2. Basil
//      Used: 8, Missed: 0
//   3. Mozzarella Cheese
//      Used: 3, Missed: 1
// ========================================

// ============================================================================
// EXAMPLE 3: Accessing Recipe Properties for Frontend Display
// ============================================================================

// Get recipe details for UI rendering
string title = margheritaRecipe.GetTitle();           // "Margherita Pizza"
string dishImage = margheritaRecipe.GetImageUrl();    // URL to dish image
double prepTime = margheritaRecipe.GetPrepTime();     // 30 minutes
int ingredientCount = margheritaRecipe.GetIngredientCount(); // 3

// Display recipe card in frontend
Console.WriteLine($"<RecipeCard>");
Console.WriteLine($"  <Title>{title}</Title>");
Console.WriteLine($"  <DishImage>{dishImage}</DishImage>");
Console.WriteLine($"  <PrepTime>{prepTime} minutes</PrepTime>");
Console.WriteLine($"  <IngredientCount>{ingredientCount}</IngredientCount>");
Console.WriteLine($"</RecipeCard>");

// ============================================================================
// EXAMPLE 4: Accessing Individual Ingredient Properties
// ============================================================================

var recipeIngredients = margheritaRecipe.GetIngredients();

foreach (var ingredient in recipeIngredients)
{
    // Display ingredient in frontend list
    Console.WriteLine($"Ingredient: {ingredient.ingredientName}");
    Console.WriteLine($"Used Count: {ingredient.usedCount}");
    Console.WriteLine($"Missed Count: {ingredient.missedCount}");
    Console.WriteLine($"Success Rate: {ingredient.GetSuccessRate():F1}%");
    Console.WriteLine($"Total Uses: {ingredient.GetTotalCount()}");
    Console.WriteLine();
}

// ============================================================================
// EXAMPLE 5: Creating Recipe with Minimal Data
// ============================================================================

// Create recipe without image (imageUrl defaults to empty string)
var simpleRecipe = new Recipe(
    title: "Simple Salad",
    ingredients: new List<RecipeIngredient>
    {
        new RecipeIngredient("Lettuce"),
        new RecipeIngredient("Tomato"),
        new RecipeIngredient("Cucumber")
    },
    time: 10
);

// Add image later if needed
string imageUrl = simpleRecipe.GetImageUrl(); // Returns "" if not provided

// ============================================================================
// EXAMPLE 6: Tracking Ingredient Success/Failure
// ============================================================================

// When recipe is prepared with an ingredient successfully used
tomatoIngredient.IncrementUsedCount();

// When an ingredient is missing from the fridge
tomatoIngredient.IncrementMissedCount();

// Get success metrics
double successRate = tomatoIngredient.GetSuccessRate(); // Percentage (0-100)
int totalAttempts = tomatoIngredient.GetTotalCount();   // Total uses + misses

Console.WriteLine($"Tomato Success Rate: {successRate:F1}% ({tomatoIngredient.usedCount}/{totalAttempts})");

// ============================================================================
// EXAMPLE 7: Generating Shopping List
// ============================================================================

var virtualFridge = new VirtualFridge();
var shoppingList = margheritaRecipe.GenerateShoppingList(virtualFridge);

var manager = new ShoppingListManager();
manager.ExportToText(shoppingList, "shopping_list.txt");

// ============================================================================
// FRONTEND DISPLAY SUGGESTIONS
// ============================================================================

/*
Recipe Card Component:
- Display dish image (margheritaRecipe.GetImageUrl())
- Display title, prep time, ingredient count
- List ingredients with their tracking stats

Frontend Example Structure:
<RecipeCard>
  <DishImage src="{recipe.GetImageUrl()}" />
  <Title>{recipe.GetTitle()}</Title>
  <PrepTime>{recipe.GetPrepTime()} min</PrepTime>

  <IngredientsList>
    {foreach ingredient in recipe.GetIngredients()}
      <IngredientItem>
        <Name>{ingredient.ingredientName}</Name>
        <Stats>
          <UsedCount>{ingredient.usedCount}</UsedCount>
          <MissedCount>{ingredient.missedCount}</MissedCount>
          <SuccessRate>{ingredient.GetSuccessRate()}%</SuccessRate>
        </Stats>
      </IngredientItem>
  </IngredientsList>
</RecipeCard>

This structure provides:
- Single dish image for better visual presentation
- Ingredient list with individual tracking
- Success rate analytics for each ingredient
*/

// ============================================================================
// BENEFITS OF THIS STRUCTURE
// ============================================================================

/*
1. Simplified Image Management: One image per recipe (the dish), not per ingredient
2. Rich Ingredient Tracking: Each ingredient tracks used/missed counts
3. Analytics Ready: Success rates show ingredient availability patterns
4. Frontend Optimized: All data available for rich UI components
5. Type Safe: RecipeIngredient provides strong typing instead of strings
6. Extensible: Easy to add more properties (difficulty, servings, calories, etc.)
*/

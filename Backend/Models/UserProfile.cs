using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Pseuchef.Enums;
using Pseuchef.Interfaces;
using Pseuchef.Services.Strategies;

namespace Pseuchef.Models
{
    public class UserProfile
    {
        private string username;
        private List<DietaryRestriction> dietaryPreferences;
        private List<string> excludedIngredients;
        private double calorieGoal;
        private bool isMetric;
        private IDietaryRestrictionStrategy dietaryStrategy;
        private IFoodSafetyStrategy foodSafetyStrategy;

        public UserProfile(string username, List<DietaryRestriction> dietaryPreferences, List<string> excludedIngredients, double calorieGoal, bool isMetric)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));
            if (calorieGoal < 0)
                throw new ArgumentException("Calorie goal cannot be negative", nameof(calorieGoal));

            this.username = username;
            this.dietaryPreferences = dietaryPreferences ?? new List<DietaryRestriction>();
            this.excludedIngredients = excludedIngredients ?? new List<string>();
            this.calorieGoal = calorieGoal;
            this.isMetric = isMetric;

            // Initialize default strategies
            this.dietaryStrategy = new DefaultDietaryRestrictionStrategy();
            this.foodSafetyStrategy = new ExcludedIngredientsCheckStrategy(this.excludedIngredients);
        }

        public void SetDietaryRestrictionStrategy(IDietaryRestrictionStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));
            this.dietaryStrategy = strategy;
        }

        public void SetFoodSafetyStrategy(IFoodSafetyStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));
            this.foodSafetyStrategy = strategy;
        }
        public void AddPreference(DietaryRestriction restriction)
        {
            if (!dietaryPreferences.Contains(restriction))
            {
                this.dietaryPreferences.Add(restriction);
                SyncExcludedIngredients();
            }
        }

        public void RemovePreference(DietaryRestriction restriction)
        {
            if (dietaryPreferences.Contains(restriction))
            {
                this.dietaryPreferences.Remove(restriction);
                SyncExcludedIngredients();
            }
        }

        public IReadOnlyList<DietaryRestriction> GetDietaryPreferences()
        {
            return dietaryPreferences.AsReadOnly();
        }

        public void SyncExcludedIngredients()
        {
            this.excludedIngredients.Clear();

            foreach (DietaryRestriction preference in dietaryPreferences)
            {
                var exclusions = dietaryStrategy.GetExcludedIngredients(preference);
                AddUniqueExclusions(exclusions);
            }

            this.foodSafetyStrategy = new ExcludedIngredientsCheckStrategy(this.excludedIngredients);
        }

        private void AddUniqueExclusions(List<string> ingredients)
        {
            foreach (string ingredient in ingredients)
            {
                if (!excludedIngredients.Contains(ingredient))
                {
                    excludedIngredients.Add(ingredient);
                }
            }
        }

        public IReadOnlyList<string> GetExcludedIngredients()
        {
            return excludedIngredients.AsReadOnly();
        }

        public bool CheckSafety(FoodItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return foodSafetyStrategy.IsSafe(item);
        }

        public string GetAPIFilterString()
        {
            if (dietaryPreferences.Count == 0)
                return string.Empty;

            return string.Join(",", dietaryPreferences.Select(d => d.ToString().ToLower()));
        }

        public string GetDietarySummary()
        {
            if (dietaryPreferences.Count == 0)
                return "No dietary restrictions";

            var summaryLines = dietaryPreferences
                .Select(pref => dietaryStrategy.GetDescription(pref))
                .ToList();

            return string.Join(Environment.NewLine, summaryLines);
        }

        public void SaveProfile()
        {
            try
            {
                string path = $"{username}_profile.txt";
                string content = $"Username: {username}\n" +
                                $"Dietary Preferences: {string.Join(",", dietaryPreferences)}\n" +
                                $"Excluded Ingredients: {string.Join(",", excludedIngredients)}\n" +
                                $"Calorie Goal: {calorieGoal}\n" +
                                $"Is Metric: {isMetric}";

                File.WriteAllText(path, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving profile: " + ex.Message);
            }
        }

        public string GetUsername() => username;

        public double GetCalorieGoal() => calorieGoal;

        public void SetCalorieGoal(double goal)
        {
            if (goal < 0)
                throw new ArgumentException("Calorie goal cannot be negative", nameof(goal));
            this.calorieGoal = goal;
        }

        public bool IsMetric() => isMetric;
        public void SetMetricPreference(bool metric)
        {
            this.isMetric = metric;
        }
    }
}

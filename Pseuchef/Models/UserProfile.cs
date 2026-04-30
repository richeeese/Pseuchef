using System;
using System.Collections.Generic;
using System.Text;

namespace Pseuchef.Models
{
    internal class UserProfile
    {
        private string username;
        private List<DietaryRestriction> dietaryPref;
        private List<string> excludedIgredients;
        private double calorieGoal;
        private bool isMetric;

        UserProfile(string username, List<DietaryRestriction> dietaryPref, List<string> excludedIgredients, double calorieGoal, bool isMetric)
        {
            this.username = username;
            this.dietaryPref = dietaryPref;
            this.excludedIgredients = excludedIgredients;
            this.calorieGoal = calorieGoal;
            this.isMetric = isMetric;
        }

        public void addPreference(DietaryRestriction)
        {
            this.dietaryPref.Add(DietaryRestriction);
        }

        public bool checkSafety(FoodItem)
        {

        }

        public string getAPIFilterString()
        {

        }

        public void saveProfile()
        {

        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pseuchef.Enums;

namespace Pseuchef.Models
{
    public class NutrientProfile
    {
        private Dictionary<NutrientType, NutrientValue> nutrients;

        public NutrientProfile(Dictionary<NutrientType, NutrientValue> nutrients = null)
        {
            this.nutrients = nutrients ?? new Dictionary<NutrientType, NutrientValue>();
        }

        public void SetNutrient(NutrientType nutrientType, double amount, string unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
                throw new ArgumentException("Unit cannot be empty", nameof(unit));
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            nutrients[nutrientType] = new NutrientValue(amount, unit);
        }

        public double GetAmount(NutrientType nutrient)
        {
            if (nutrients.TryGetValue(nutrient, out var value))
                return value.Amount;

            return 0;
        }

        public string GetUnit(NutrientType nutrient)
        {
            if (nutrients.TryGetValue(nutrient, out var value))
                return value.Unit;

            return string.Empty;
        }

        public NutrientValue GetNutrient(NutrientType nutrient)
        {
            if (nutrients.TryGetValue(nutrient, out var value))
                return value;

            return null;
        }

        public string GetSummary()
        {
            if (!nutrients.Any())
                return "No nutrients tracked.";

            var summaryLines = nutrients
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => $"{kvp.Key}: {kvp.Value}")
                .ToList();

            return string.Join(Environment.NewLine, summaryLines);
        }

        public IReadOnlyDictionary<NutrientType, NutrientValue> GetAllNutrients()
        {
            return nutrients.AsReadOnly();
        }

        public bool HasNutrient(NutrientType nutrient)
        {
            return nutrients.ContainsKey(nutrient);
        }

        public override string ToString()
        {
            return GetSummary();
        }
    }
}

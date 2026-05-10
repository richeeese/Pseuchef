using System;
using System.Collections.Generic;
using System.Text;

namespace Pseuchef.Models
{
    public class NutrientValue
    {
        public double Amount { get; private set; }
        public string Unit { get; private set; }

        public NutrientValue(double amount, string unit)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));
            if (string.IsNullOrWhiteSpace(unit))
                throw new ArgumentException("Unit cannot be empty", nameof(unit));

            this.Amount = amount;
            Unit = unit;
        }

        public override string ToString()
        {
            return $"{Amount} {Unit}";
        }

        public NutrientValue Add(NutrientValue other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (!Unit.Equals(other.Unit, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Cannot add nutrients with different units: {Unit} vs {other.Unit}");

            return new NutrientValue(Amount + other.Amount, Unit);
        }
    }
}
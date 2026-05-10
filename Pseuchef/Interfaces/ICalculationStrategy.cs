using System;
using System.Collections.Generic;
using Pseuchef.Models;

namespace Pseuchef.Interfaces
{
    // calculation stuff, might be disabled depending if ma-push pa ang nutrition functionality
    public interface ICalculationStrategy
    {
        double Calculate(List<FoodItem> items);

        string GetDescription();
    }
}

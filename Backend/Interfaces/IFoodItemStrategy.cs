using System;
using System.Collections.Generic;
using Pseuchef.Models;

namespace Pseuchef.Interfaces
{
    // filtering items by certain criteria
    public interface IFoodItemFilterStrategy
    {
        List<FoodItem> Filter(List<FoodItem> items);

        string GetDescription();
    }
}

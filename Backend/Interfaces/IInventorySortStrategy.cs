using System;
using System.Collections.Generic;
using Pseuchef.Models;

namespace Pseuchef.Interfaces
{
    // sorting inventory by certain criteria
    public interface IInventorySortStrategy
    {
        List<FoodItem> Sort(List<FoodItem> items);

        string GetDescription();
    }
}

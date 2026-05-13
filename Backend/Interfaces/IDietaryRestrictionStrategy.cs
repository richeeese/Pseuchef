using System;
using System.Collections.Generic;
using Pseuchef.Enums;

namespace Pseuchef.Interfaces
{
    // manages dietary restrictions
    public interface IDietaryRestrictionStrategy
    {
        List<string> GetExcludedIngredients(DietaryRestriction restriction);

        string GetDescription(DietaryRestriction restriction);
    }
}

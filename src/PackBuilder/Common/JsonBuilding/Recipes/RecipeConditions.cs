using PackBuilder.Content.JsonBuilding.Recipes.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes;

// The condition(s) needing to be met in order for this mod to activate.
// If no conditions are specified, this mod will activate on ALL recipes.

internal class RecipeConditions
{
    public List<RecipeCondition> Conditions = [];

    // All of these are available as set only properties so that the json parser
    // can continually build a list by specifying the same property multiple times.
    //
    // Although there may be better ways to do this for strictly programming, doing it
    // this way creates the cleaniest, most intuitive, and easiest implementation
    // for creating json files.

    public RequiresIngredient ContainsItem { set => Conditions.Add(value); }
    public CreatesResult CreatesItem { set => Conditions.Add(value); }

    /// <summary>
    /// Determines whether this <see cref="RecipeConditions"/> set applies to a given recipe.
    /// </summary>
    public bool AppliesTo(Recipe recipe, RecipeCriteria criteria)
    {
        if (Conditions.Count == 0)
            throw new MissingFieldException("Must specify 1 or more conditions for a recipe modification!", nameof(Conditions));

        if (criteria == RecipeCriteria.Any)
            return Conditions.Any(c => c.AppliesTo(recipe));

        return Conditions.All(c => c.AppliesTo(recipe));
    }
}

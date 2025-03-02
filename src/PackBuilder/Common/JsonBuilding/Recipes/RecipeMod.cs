﻿using Terraria;

namespace PackBuilder.Common.JsonBuilding.Recipes
{
    internal class RecipeMod
    {
        // Either All or Any.
        // If "All" is specified, ALL of the conditions will need to be met in order to activate the changes of this mod.
        // If "Any" is specified, ANY of the conditions being met will activate the changes of this mod.
        public RecipeCriteria Criteria { get; set; } = RecipeCriteria.All;

        // The condition(s) needing to be met in order for this mod to activate.
        public required RecipeConditions Conditions { get; set; }

        // The change(s) that will be applied to each of the recipes where conditions are met.
        public required RecipeChanges Changes { get; set; }

        /// <summary>
        /// Tests conditions and applies this <see cref="RecipeMod"/> to every loaded <see cref="Recipe"/>.
        /// </summary>
        public void Apply()
        {
            foreach (var recipe in Main.recipe)
            {
                // 'applies' will be true in any of the following cases:
                //      - There are no specified conditions.
                //      - The specified criteria is "all" and ALL specified conditions are met.
                //      - The specified criteria is "any" and ANY single specified condition is met.
                bool applies = Conditions.AppliesTo(recipe, Criteria);

                // If this mod does not apply to a given recipe, move to the next.
                if (!applies)
                    continue;

                // Apply this recipe mod's changes.
                Changes.ApplyTo(recipe);
            }
        }
    }
}

using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Conditions
{
    internal abstract class RecipeCondition
    {
        public abstract bool AppliesTo(Recipe recipe);
    }
}

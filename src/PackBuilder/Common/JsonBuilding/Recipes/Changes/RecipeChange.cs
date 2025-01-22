using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Changes
{
    internal abstract class RecipeChange
    {
        public abstract void ApplyTo(Recipe recipe);
    }
}

using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Conditions
{
    internal class CreatesResult : RecipeCondition
    {
        public required string Item;

        public int Count = -1;

        public override bool AppliesTo(Recipe recipe)
        {
            int item = GetItem(Item);

            if (Count == -1)
                return recipe.HasResult(item);

            return recipe.TryGetResult(item, out var ingredient) && ingredient.stack == Count;
        }
    }
}

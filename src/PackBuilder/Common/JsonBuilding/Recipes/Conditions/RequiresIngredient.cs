using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Conditions
{
    internal class RequiresIngredient : RecipeCondition
    {
        public required string Item;

        public int Count = -1;

        public override bool AppliesTo(Recipe recipe)
        {
            int item = GetItem(Item);

            if (Count == -1)
                return recipe.HasIngredient(item);

            return recipe.TryGetIngredient(item, out var ingredient) && ingredient.stack == Count;
        }
    }
}

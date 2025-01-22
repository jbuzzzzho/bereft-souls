using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Changes
{
    internal class RemoveIngredient : RecipeChange
    {
        public required string Item;

        public override void ApplyTo(Recipe recipe)
        {
            int item = GetItem(Item);
            recipe.RemoveIngredient(item);
        }
    }
}

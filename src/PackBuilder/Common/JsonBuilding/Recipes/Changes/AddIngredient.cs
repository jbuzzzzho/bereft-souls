using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Changes
{
    internal class AddIngredient : RecipeChange
    {
        public required string Item;

        public int Count = 1;

        public override void ApplyTo(Recipe recipe)
        {
            int item = GetItem(Item);
            recipe.AddIngredient(item, Count);
        }
    }
}

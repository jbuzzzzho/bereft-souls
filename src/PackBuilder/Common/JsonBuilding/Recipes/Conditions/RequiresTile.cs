using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Conditions
{
    internal class RequiresTile : RecipeCondition
    {
        public required string Tile;

        public override bool AppliesTo(Recipe recipe)
        {
            int tile = GetTile(Tile);
            return recipe.HasTile(tile);
        }
    }
}

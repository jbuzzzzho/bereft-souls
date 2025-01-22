using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Changes
{
    internal class AddTile : RecipeChange
    {
        public required string Tile;

        public override void ApplyTo(Recipe recipe)
        {
            int tile = GetTile(Tile);
            recipe.AddTile(tile);
        }
    }
}

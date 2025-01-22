using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Changes
{
    internal class ChangeTile : RecipeChange
    {
        public string OldTile = null;

        public required string Tile;

        public override void ApplyTo(Recipe recipe)
        {
            int oldTile = OldTile == null ? -1 : GetTile(OldTile);
            int newTile = GetTile(Tile);

            if (oldTile == -1)
                recipe.requiredTile.Clear();

            else
                recipe.RemoveTile(oldTile);

            recipe.AddTile(newTile);
        }
    }
}

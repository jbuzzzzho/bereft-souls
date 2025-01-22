using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Changes
{
    internal class AddRecipeGroup : RecipeChange
    {
        public required string Group;

        public int Stack = 1;

        public override void ApplyTo(Recipe recipe)
        {
            int id = GetRecipeGroup(Group);
            recipe.AddRecipeGroup(id, Stack);
        }
    }
}

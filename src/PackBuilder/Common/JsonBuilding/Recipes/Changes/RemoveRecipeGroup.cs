using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Changes
{
    internal class RemoveRecipeGroup : RecipeChange
    {
        public required string Group;

        public override void ApplyTo(Recipe recipe)
        {
            int id = GetRecipeGroup(Group);
            RecipeGroup group = RecipeGroup.recipeGroups[id];

            recipe.RemoveRecipeGroup(id);
            recipe.RemoveIngredient(group.IconicItemId);
        }
    }
}

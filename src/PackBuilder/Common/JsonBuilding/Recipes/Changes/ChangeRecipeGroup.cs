using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Changes
{
    internal class ChangeRecipeGroup : RecipeChange
    {
        public required string Group;

        public string NewGroup = null;

        public int NewCount = 1;

        public override void ApplyTo(Recipe recipe)
        {
            int id = GetRecipeGroup(Group);
            RecipeGroup group = RecipeGroup.recipeGroups[id];
            int newId = id;

            if (NewGroup is not null)
                newId = GetRecipeGroup(NewGroup);

            recipe.RemoveRecipeGroup(id);
            recipe.RemoveIngredient(group.IconicItemId);

            recipe.AddRecipeGroup(newId, NewCount);
        }
    }
}

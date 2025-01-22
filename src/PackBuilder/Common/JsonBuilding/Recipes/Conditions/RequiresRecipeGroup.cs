using Terraria;

namespace PackBuilder.Content.JsonBuilding.Recipes.Conditions
{
    internal class RequiresRecipeGroup : RecipeCondition
    {
        public required string Group;

        public override bool AppliesTo(Recipe recipe)
        {
            int id = GetRecipeGroup(Group);
            return recipe.HasRecipeGroup(id);
        }
    }
}

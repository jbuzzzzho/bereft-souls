using FargowiltasSouls.Content.Items.Accessories.Enchantments;

using Microsoft.Xna.Framework;

using SOTS.Items.Nature;

using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace BereftSouls.Content.Items.Accessories;

internal sealed class WormwoodEnchant : BaseEnchant
{
    private const int defense_boost   = 1;
    private const int hooks_to_summon = 4;

    public override Color nameColor => new(100, 173, 255);

    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
        defense_boost
    );

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.rare  = ItemRarityID.Blue;
        Item.value = Item.sellPrice(gold: 5);
    }

    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe()
           .AddIngredient<NatureWreath>()
           .AddIngredient<NatureShirt>()
           .AddIngredient<NatureLeggings>()
           .AddIngredient<BotanicalSymbiote>()
           .AddIngredient<NatureSpell>()
           .AddIngredient(ItemID.EmeraldStaff)
           .AddTile(TileID.DemonAltar)
           .Register();
    }
}
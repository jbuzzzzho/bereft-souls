using FargowiltasSouls.Content.Items.Accessories.Enchantments;

using Microsoft.Xna.Framework;

using SOTS.Items.Earth;

using Terraria;
using Terraria.ID;

namespace BereftSouls.Content.Items.Accessories;

internal sealed class VibrantEnchant : BaseEnchant
{
    public override Color nameColor => new(181, 220, 97);
    
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.rare  = ItemRarityID.Green;
        Item.value = Item.sellPrice(gold: 5);
    }
    
    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe()
           .AddIngredient<VibrantHelmet>()
           .AddIngredient<VibrantChestplate>()
           .AddIngredient<VibrantLeggings>()
           .AddIngredient<HarvestersScythe>()
           .AddIngredient<EchoDisk>()
           .AddIngredient<VibrantStaff>()
           .AddTile(TileID.DemonAltar)
           .Register();
    }
}
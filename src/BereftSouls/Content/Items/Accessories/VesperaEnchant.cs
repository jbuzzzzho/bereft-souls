using BereftSouls.Common.Keybinds;

using FargowiltasSouls.Content.Items.Accessories.Enchantments;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using SOTS.Items.Invidia;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BereftSouls.Content.Items.Accessories;

internal sealed class VesperaEnchant : BaseEnchant
{
    private sealed class Keybind : AbstractKeybind
    {
        protected override string Name => nameof(VesperaEnchant) + "Keybind";

        protected override Keys DefaultBinding => Keys.None;
    }

    public override Color nameColor => new(111, 236, 179);

    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
        ModContent.GetInstance<Keybind>().Keybind!.DisplayName.Value
    );

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
           .AddIngredient<VesperaMask>()
           .AddIngredient<VesperaBreastplate>()
           .AddIngredient<VesperaLeggings>()
           .AddIngredient<VesperaNanDao>()
           .AddIngredient<VesperaLongbow>()
           .AddIngredient<VesperaFishingRod>()
           .AddTile(TileID.DemonAltar)
           .Register();
    }
}
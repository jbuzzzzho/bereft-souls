using FargowiltasSouls.Content.Items.Accessories.Enchantments;

using Microsoft.Xna.Framework;

using Terraria.Localization;

namespace BereftSouls.Content.Items.Accessories;

internal sealed class FrigidEnchant : BaseEnchant
{
    private const int percent_damage = 10;
    private const int flat_damage    = 10;
    private const int crit           = 5;

    public override Color nameColor => new(60, 43, 206);

    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
        percent_damage,
        flat_damage,
        crit
    );
}
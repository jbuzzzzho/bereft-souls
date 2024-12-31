using FargowiltasSouls.Content.Items.Accessories.Enchantments;

using Microsoft.Xna.Framework;

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
}
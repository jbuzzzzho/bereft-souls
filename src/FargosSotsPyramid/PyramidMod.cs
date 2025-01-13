using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace FargosSotsPyramid;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public sealed class PyramidMod;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class PyramidSystem : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        base.ModifyWorldGenTasks(tasks, ref totalWeight);

        // We want to stop Fargo's Souls from running their pyramid generation
        // routine since we take over and integrate it into SotS.
        // https://github.com/Fargowilta/FargowiltasSouls/blob/0dc3549461593b85eac3f4c226270ea601a526d1/Core/Systems/PyramidGenSystem.cs#L243
        FindAndDisablePass("GuaranteePyramid");      // Simply guarantees a pyramid.
        FindAndDisablePass("GuaranteePyramidAgain"); // Also just guarantees a pyramid.
        FindAndDisablePass("CursedCoffinArena");     // The actual pass responsible for generating the arena; most important.

        return;

        void FindAndDisablePass(string name)
        {
            foreach (var task in tasks)
            {
                if (task.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Non-destructive removal with disable.
                    task.Disable();
                }

                return;
            }

            Mod.Logger.Warn($"Failed to find and disable '{name}' generation pass.");
        }
    }
}

// The original plan was to allow tiles to be broken whenever, but we should
// treat it like the dungeon.
/*
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class PyramidGlobalTile : GlobalTile
{
    private static readonly Lazy<int[]> pyramid_tiles = new(
        () =>
        [
            ModContent.TileType<PyramidSlabTile>(),
            ModContent.TileType<PyramidRubbleTile>(),
            ModContent.TileType<OvergrownPyramidTile>(),
        ]
    );

    public override bool Slope(int i, int j, int type)
    {
        return pyramid_tiles.Value.Contains(type) || base.Slope(i, j, type);
    }

    public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
    {
        return pyramid_tiles.Value.Contains(type) || base.CanKillTile(i, j, type, ref blockDamaged);
    }
}
*/
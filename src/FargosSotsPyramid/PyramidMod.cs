using System;
using System.Collections.Generic;
using System.Reflection;

using FargowiltasSouls.Core.Systems;

using JetBrains.Annotations;

using Mono.Cecil.Cil;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;

using SOTS;
using SOTS.Buffs;
using SOTS.Common.GlobalNPCs;
using SOTS.Items.Pyramid;
using SOTS.WorldgenHelpers;

using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace FargosSotsPyramid;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public sealed class PyramidMod;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class PyramidSystem : ModSystem
{
    private ILHook? pyramidGateHook;
    private ILHook? disableEarlyPyramidSpawns;
    private ILHook? disableFargosPyramidGen;

    public override void Load()
    {
        base.Load();

        pyramidGateHook = new ILHook(
            typeof(PyramidWorldgenHelper).GetMethod(nameof(PyramidWorldgenHelper.GenerateSOTSPyramid), BindingFlags.Public | BindingFlags.Static)!,
            GenerateSotsPyramid_RemovePyramidGate
        );

        disableEarlyPyramidSpawns = new ILHook(
            typeof(SOTSNPCs).GetMethod(nameof(SOTSNPCs.EditSpawnPool), BindingFlags.Public | BindingFlags.Instance)!,
            EditSpawnPool_DisableZonePyramidSpawnsEarly
        );

        disableFargosPyramidGen = new ILHook(
            typeof(PyramidGenSystem).GetMethod(nameof(PyramidGenSystem.ModifyWorldGenTasks), BindingFlags.Public | BindingFlags.Instance)!,
            ModifyWorldGenTasks_DisableFargosPyramidGen
        );
    }

    public override void Unload()
    {
        base.Unload();

        pyramidGateHook?.Dispose();
        pyramidGateHook = null;
        disableEarlyPyramidSpawns?.Dispose();
        disableEarlyPyramidSpawns = null;
        disableFargosPyramidGen?.Dispose();
        disableFargosPyramidGen = null;
    }

    private static void GenerateSotsPyramid_RemovePyramidGate(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.After,
            x => x.MatchCall(
                typeof(ModContent).GetMethod(nameof(ModContent.TileType), BindingFlags.Public | BindingFlags.Static)!
                                  .MakeGenericMethod(typeof(PyramidGateTile))
            )
        );

        c.GotoNext(MoveType.Before, x => x.MatchCall<WorldGen>(nameof(WorldGen.PlaceTile)));
        c.Remove();
        c.Emit(OpCodes.Pop); // 7 parameters in PlaceTile.
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Pop);
        // c.Emit(OpCodes.Pop); // Omit one that remains from popping the return value of PlaceTile.
    }

    private static void EditSpawnPool_DisableZonePyramidSpawnsEarly(ILContext il)
    {
        var c = new ILCursor(il);

        // Very simple check that that modifies handling of PyramidBiome in the
        // method to only be true if the world evil boss has been defeated.
        // This is to prevent NPCs from spawning early if the player is there to
        // fight the Fargo's boss.
        c.GotoNext(MoveType.After, x => x.MatchCallvirt<SOTSPlayer>("get_PyramidBiome"));
        c.EmitDelegate(
            (bool pyramidBiome) => NPC.downedBoss2 && pyramidBiome
        );
    }

    private static void ModifyWorldGenTasks_DisableFargosPyramidGen(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After, x => x.MatchCall<PyramidGenSystem>("get_ShouldGenerateArena"));
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Ldc_I4_0);
    }
}

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class PyramidPlayer : ModPlayer
{
    public override void PreUpdateBuffs()
    {
        base.PreUpdateBuffs();

        Player.buffImmune[ModContent.BuffType<PharaohsCurse>()] = true;
    }

    public override void PostUpdateBuffs()
    {
        base.PostUpdateBuffs();
    }
}

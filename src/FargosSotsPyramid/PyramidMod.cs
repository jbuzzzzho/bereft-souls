using System;
using System.Reflection;

using FargowiltasSouls.Core.Systems;

using JetBrains.Annotations;

using Mono.Cecil.Cil;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;

using SOTS;
using SOTS.Biomes;
using SOTS.Common.GlobalNPCs;
using SOTS.Items.Pyramid;
using SOTS.WorldgenHelpers;

using Terraria;
using Terraria.ModLoader;

namespace FargosSotsPyramid;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public sealed class PyramidMod;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class PyramidSystem : ModSystem
{
    private IDisposable? pyramidGateHook;
    private IDisposable? disableEarlyPyramidSpawns;
    private IDisposable? disableFargosPyramidGen;
    private IDisposable? disablePyramidBiomeBeforeEvilBoss;

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

        disablePyramidBiomeBeforeEvilBoss = new Hook(
            typeof(PyramidBiome).GetMethod(nameof(PyramidBiome.IsBiomeActive), BindingFlags.Public | BindingFlags.Instance)!,
            IsBiomeActive_DisablePyramidBiomeBeforeEvilBoss
        );
    }

    public override void Unload()
    {
        base.Unload();

        DisposeOf(ref pyramidGateHook);
        DisposeOf(ref disableEarlyPyramidSpawns);
        DisposeOf(ref disableFargosPyramidGen);
        DisposeOf(ref disablePyramidBiomeBeforeEvilBoss);

        return;

        static void DisposeOf(ref IDisposable? disposable)
        {
            disposable?.Dispose();
            disposable = null;
        }
    }

    // ReSharper disable once InconsistentNaming
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

    // ReSharper disable once InconsistentNaming
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

    // ReSharper disable once InconsistentNaming
    private static void ModifyWorldGenTasks_DisableFargosPyramidGen(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After, x => x.MatchCall<PyramidGenSystem>("get_ShouldGenerateArena"));
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Ldc_I4_0);
    }

    // ReSharper disable once InconsistentNaming
    private static bool IsBiomeActive_DisablePyramidBiomeBeforeEvilBoss(
        Func<PyramidBiome, Player, bool> orig,
        PyramidBiome                     self,
        Player                           player
    )
    {
        return NPC.downedBoss2 && orig(self, player);
    }
}
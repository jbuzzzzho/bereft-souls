using System;
using System.Reflection;

using EditDecompiler;

using FargowiltasSouls.Core.Systems;

using JetBrains.Annotations;

using Mono.Cecil.Cil;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;

using SOTS;
using SOTS.Biomes;
using SOTS.Common.GlobalNPCs;
using SOTS.Items.Pyramid;
using SOTS.Items.Pyramid.PyramidWalls;
using SOTS.WorldgenHelpers;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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

internal sealed class TemporaryClassForModifyingTheStructure : ModSystem
{
    private static IDisposable? a;

    public override void PostSetupContent()
    {
        base.PostSetupContent();

        var mod    = ModLoader.GetMod("StructureHelper");
        var type   = mod.Code.GetType("StructureHelper.Generator")!;
        var method = type.GetMethod("Generate", BindingFlags.Public | BindingFlags.Static, null, [typeof(TagCompound), typeof(Terraria.DataStructures.Point16), typeof(bool), mod.Code.GetType("StructureHelper.GenFlags")!], null)!;
        a = new ILHook(
            // typeof(StructureHelper.Generator).GetMethod("Generate", BindingFlags.Public | BindingFlags.Static, null, [typeof(TagCompound), typeof(Terraria.DataStructures.Point16), typeof(bool), typeof(StructureHelper.GenFlags)], null)!,
            method,
            il =>
            {
                var c = new ILCursor(il);

                var typeIndex = -1;
                c.GotoNext(x => x.MatchCallvirt<ModBlockType>("get_Type"));
                c.GotoNext(x => x.MatchStloc(out typeIndex));

                var wallTypeIndex = -1;
                c.GotoNext(x => x.MatchCallvirt<ModBlockType>("get_Type"));
                c.GotoNext(x => x.MatchStloc(out wallTypeIndex));

                if (typeIndex == -1 || wallTypeIndex == -1)
                {
                    throw new Exception("Failed to find the local variable storing the type of the block.");
                }

                c.GotoNext(x => x.MatchCall<StructureHelper.TileSaveData>("get_Active"));
                c.GotoNext(MoveType.AfterLabel, x => x.MatchLdloc(out _));

                c.Emit(OpCodes.Ldloc, typeIndex);
                c.EmitDelegate(ReplaceTileType);
                c.Emit(OpCodes.Stloc, typeIndex);

                c.Emit(OpCodes.Ldloc, wallTypeIndex);
                c.EmitDelegate(ReplaceWallType);
                c.Emit(OpCodes.Stloc, wallTypeIndex);

                // MonoModHooks.DumpIL(Mod, il);
                TheDecompiler.DecompileAndDump(Mod, il);

                return;

                static int ReplaceTileType(int type)
                {
                    if (type == TileID.SandstoneBrick)
                    {
                        type = ModContent.TileType<PyramidSlabTile>();
                    }

                    return type;
                }

                static int ReplaceWallType(int type)
                {
                    if (type == WallID.SandstoneBrick)
                    {
                        type = ModContent.WallType<UnsafePyramidWallWall>();
                    }

                    return type;
                }
            }
        );
    }
}
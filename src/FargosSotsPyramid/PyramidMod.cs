using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using FargowiltasSouls.Content.Bosses.CursedCoffin;
using FargowiltasSouls.Content.WorldGeneration;
using FargowiltasSouls.Core.Systems;

using JetBrains.Annotations;

using Microsoft.Xna.Framework;

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
    private IDisposable? spawnFargosPyramidBossInSotsRoom;

    private readonly List<IDisposable> hooksToPatchConstantWidthAndHeight = [];

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

        spawnFargosPyramidBossInSotsRoom = new Hook(
            typeof(PyramidWorldgenHelper).GetMethod(nameof(PyramidWorldgenHelper.GenerateBossRoom), BindingFlags.Public | BindingFlags.Static)!,
            GenerateBossRoom_SpawnFargosBossOnSarcophagus
        );

        // CursedCoffin.SlamWShockwave
        // CursedCoffin.WavyShotSlam
        // CursedCoffin.RandomStuff
        // CursedCoffin.Targeting
        // CoffinArena.SetArenaPosition
        // CoffinArena.ClampWithinArena
        // CoffinArena.ArenaCorners
        // CoffinArena.TopArenaCorners
        // CursedCoffin.AI
        // Need local method Position but I'm lazy
        // WorldUpdatingSystem.PostUpdateWorld
        hooksToPatchConstantWidthAndHeight.AddRange(
            MakeWidthHeightHooks(
                typeof(CursedCoffin).GetMethod("SlamWShockwave", BindingFlags.NonPublic         | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!,
                typeof(CursedCoffin).GetMethod("WavyShotSlam",   BindingFlags.NonPublic         | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!,
                typeof(CursedCoffin).GetMethod("RandomStuff",    BindingFlags.NonPublic         | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!,
                typeof(CursedCoffin).GetMethod("Targeting",      BindingFlags.NonPublic         | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!,
                typeof(CoffinArena).GetMethod("SetArenaPosition", BindingFlags.NonPublic        | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!,
                typeof(CoffinArena).GetMethod("ClampWithinArena", BindingFlags.NonPublic        | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!,
                typeof(CoffinArena).GetMethod("ArenaCorners",     BindingFlags.NonPublic        | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!,
                typeof(CoffinArena).GetMethod("TopArenaCorners",  BindingFlags.NonPublic        | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!,
                typeof(CursedCoffin).GetMethod("AI", BindingFlags.NonPublic                     | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!,
                typeof(WorldUpdatingSystem).GetMethod("PostUpdateWorld", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!
            )
        );

        // Override Cursed Coffin to spawn at a slight offset.
        On_NPC.NewNPC += (orig, source, i, i1, type, start, ai0, ai1, ai2, ai3, target) =>
        {
            if (type != ModContent.NPCType<CursedCoffinInactive>())
            {
                return orig(source, i, i1, type, start, ai0, ai1, ai2, ai3, target);
            }
            
            var npcIndex = orig(source, i, i1, type, start, ai0, ai1, ai2, ai3, target);
            {
                Debug.Assert(npcIndex >= 0 && npcIndex <= Main.maxNPCs);
            }
            
            var npc = Main.npc[npcIndex];
            {
                Debug.Assert(npc.type == ModContent.NPCType<CursedCoffinInactive>());
            }

            // TODO: I forget if we need to bother syncing this.
            npc.position -= new Vector2(16f / 2f, 16f * 8f);

            return npcIndex;
        };

        return;

        static IEnumerable<ILHook> MakeWidthHeightHooks(params MethodInfo[] methods)
        {
            return methods.Select(x => new ILHook(x, PatchConstantWidthAndHeight));
        }
    }

    public override void Unload()
    {
        base.Unload();

        DisposeOf(ref pyramidGateHook);
        DisposeOf(ref disableEarlyPyramidSpawns);
        DisposeOf(ref disableFargosPyramidGen);
        DisposeOf(ref disablePyramidBiomeBeforeEvilBoss);
        DisposeOf(ref spawnFargosPyramidBossInSotsRoom);

        foreach (var hook in hooksToPatchConstantWidthAndHeight)
        {
            hook.Dispose();
        }

        hooksToPatchConstantWidthAndHeight.Clear();

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

    // ReSharper disable once InconsistentNaming
    private static void GenerateBossRoom_SpawnFargosBossOnSarcophagus(
        Action<int, int, int> orig,
        int                   spawnX,
        int                   spawnY,
        int                   direction
    )
    {
        orig(spawnX, spawnY, direction);

        CoffinArena.SetArenaPosition(new Point(spawnX, spawnY));
    }

    private static void PatchConstantWidthAndHeight(ILContext il)
    {
        var c = new ILCursor(il);

        while (c.TryGotoNext(MoveType.After, x => x.MatchLdcI4(60)))
        {
            c.EmitPop();
            c.EmitDelegate(GetWidth);
        }

        c.Index = 0;

        while (c.TryGotoNext(MoveType.After, x => x.MatchLdcI4(35)))
        {
            c.EmitPop();
            c.EmitDelegate(GetHeight);
        }

        c.Index = 0;

        while (c.TryGotoNext(MoveType.After, x => x.MatchCall("FargowiltasSouls.Content.WorldGeneration.CoffinArena", "get_VectorWidth")))
        {
            c.EmitPop();
            c.EmitDelegate(VectorWidth);
        }

        c.Index = 0;

        while (c.TryGotoNext(MoveType.After, x => x.MatchCall("FargowiltasSouls.Content.WorldGeneration.CoffinArena", "get_VectorHeight")))
        {
            c.EmitPop();
            c.EmitDelegate(VectorHeight);
        }

        return;

        static int GetWidth()
        {
            return 60 + 24 * 2;
        }

        static int GetHeight()
        {
            return 35;
        }

        static int VectorWidth()
        {
            return GetWidth() * 16;
        }

        static int VectorHeight()
        {
            return GetHeight() * 16;
        }
    }
}

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class PyramidChestLocker : GlobalTile
{
    // Treat Pyramid Chests as "locked" until the World Evil boss has been
    // downed to prevent progression skips.  Also prevents interacting with the
    // Sarcophagus for similar reasons.
    // TODO: Display a chat message when right-clicking?

    public override void Load()
    {
        base.Load();

        On_Chest.IsLocked_int_int_Tile += (orig, x, y, tile) =>
        {
            if (tile.TileType != ModContent.TileType<PyramidChestTile>())
            {
                return orig(x, y, tile);
            }

            return !NPC.downedBoss2;
        };

        On_Chest.Unlock += (orig, x, y) =>
        {
            var tile = Framing.GetTileSafely(x, y);
            if (tile.TileType != ModContent.TileType<PyramidChestTile>())
            {
                return orig(x, y);
            }

            // Refuse any attempts to unlock.  Don't care about downed
            // conditions since the player shouldn't have an opportunity to try
            // and unlock the chest once a World Evil boss is downed.
            return false;
        };
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;

using Terraria.ModLoader;

namespace BereftSouls.Common;

/// <summary>
///     Auxiliary abstraction over <see cref="ModSystem"/> to easily facilitate
///     creating and disposing of hooks.
/// </summary>
internal abstract class HookSystem : ModSystem
{
    private readonly List<IDisposable> hooks = [];

    protected void AddHook(ILHook hook)
    {
        hooks.Add(hook);
    }

    protected void AddHook(Hook hook)
    {
        hooks.Add(hook);
    }

    public override void Unload()
    {
        base.Unload();

        foreach (var hook in hooks)
        {
            Debug.Assert(hook is Hook or ILHook, "hook is Hook or ILHook");

            hook.Dispose();
        }
    }
}
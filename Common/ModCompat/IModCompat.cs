using System.Diagnostics.CodeAnalysis;

using Terraria.ModLoader;

namespace BereftSouls.Common.ModCompat;

/// <summary>
///     Defines a simple static abstraction over <see cref="Mod"/> instances.
/// </summary>
/// <typeparam name="TModCompat">Itself.</typeparam>
/// <remarks>
///     This is used to enable some type-safe usage of mod identities and as a
///     centralized API for accessing <see cref="Mod"/> instances.
/// </remarks>
public interface IModCompat<TModCompat> where TModCompat : IModCompat<TModCompat>
{
    /// <summary>
    ///     The internal name of the mod.
    /// </summary>
    static abstract string ModName { get; }

    /// <summary>
    ///     Whether the mod is loaded.
    /// </summary>
    static abstract bool IsLoaded { get; }

    /// <summary>
    ///     The instance of the mod.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Instance))]
    static abstract Mod? Instance { get; }
}
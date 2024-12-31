using Terraria.ModLoader;

namespace BereftSouls.Common.ModCompat;

internal sealed class SotsCompat : IModCompat<SotsCompat>
{
    public static string ModName => "SOTS";

    public static bool IsLoaded { get; }

    public static Mod? Instance { get; }

    static SotsCompat()
    {
        // ReSharper disable once AssignmentInConditionalExpression
        if (IsLoaded = ModLoader.TryGetMod(ModName, out var mod))
        {
            Instance = mod;
        }
    }
}

internal sealed class CalamityModCompat : IModCompat<CalamityModCompat>
{
    public static string ModName => "CalamityMod";

    public static bool IsLoaded { get; }

    public static Mod? Instance { get; }

    static CalamityModCompat()
    {
        // ReSharper disable once AssignmentInConditionalExpression
        if (IsLoaded = ModLoader.TryGetMod(ModName, out var mod))
        {
            Instance = mod;
        }
    }
}

internal sealed class FargowiltasSoulsCompat : IModCompat<FargowiltasSoulsCompat>
{
    public static string ModName => "FargowiltasSouls";

    public static bool IsLoaded { get; }

    public static Mod? Instance { get; }

    static FargowiltasSoulsCompat()
    {
        // ReSharper disable once AssignmentInConditionalExpression
        if (IsLoaded = ModLoader.TryGetMod(ModName, out var mod))
        {
            Instance = mod;
        }
    }
}

internal sealed class FargowiltasCompat : IModCompat<FargowiltasCompat>
{
    public static string ModName => "Fargowiltas";

    public static bool IsLoaded { get; }

    public static Mod? Instance { get; }

    static FargowiltasCompat()
    {
        // ReSharper disable once AssignmentInConditionalExpression
        if (IsLoaded = ModLoader.TryGetMod(ModName, out var mod))
        {
            Instance = mod;
        }
    }
}

internal sealed class ThoriumModCompat : IModCompat<ThoriumModCompat>
{
    public static string ModName => "ThoriumMod";

    public static bool IsLoaded { get; }

    public static Mod? Instance { get; }

    static ThoriumModCompat()
    {
        // ReSharper disable once AssignmentInConditionalExpression
        if (IsLoaded = ModLoader.TryGetMod(ModName, out var mod))
        {
            Instance = mod;
        }
    }
}
using System;
using System.Reflection;

using Terraria.ModLoader;

namespace BereftSouls.Common.ModCompat;

[AttributeUsage(
    AttributeTargets.Class
  | AttributeTargets.Constructor
  | AttributeTargets.Method
  | AttributeTargets.Property,
    Inherited = true,
    AllowMultiple = true
)]
public sealed class JitWhenModEnabledAttribute<TModCompat> : MemberJitAttribute where TModCompat : IModCompat<TModCompat>
{
    public override bool ShouldJIT(MemberInfo member)
    {
        return TModCompat.IsLoaded;
    }
}
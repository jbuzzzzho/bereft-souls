using Terraria;

namespace PackBuilder.Common.JsonBuilding.Projectiles.Changes
{
    internal class VanillaProjectileChange : IProjectileChange
    {
        public string? Damage { get; set; }
        public string? Piercing { get; set; }
        public string? Scale { get; set; }
        public string? HitCooldown { get; set; }

        public void ApplyTo(Projectile projectile)
        {
            static void DeltaF(ref float field, string? adjustment)
            {
                if (adjustment is null)
                    return;

                if (adjustment.StartsWith('+'))
                    field += float.Parse(adjustment.Substring(1));

                else if (adjustment.StartsWith('-'))
                    field += float.Parse(adjustment);

                else if (adjustment.StartsWith('x'))
                    field *= float.Parse(adjustment.Substring(1));

                else
                    field = float.Parse(adjustment);
            }
            static void Delta(ref int field, string? adjustment)
            {
                float val = field;
                DeltaF(ref val, adjustment);
                field = (int)val;
            }

            Delta(ref projectile.damage, Damage);
            Delta(ref projectile.maxPenetrate, Piercing);
            projectile.penetrate = projectile.maxPenetrate;
            DeltaF(ref projectile.scale, Scale);

            if (projectile.usesLocalNPCImmunity)
                Delta(ref projectile.localNPCHitCooldown, HitCooldown);

            else if (projectile.usesIDStaticNPCImmunity)
                Delta(ref projectile.idStaticNPCHitCooldown, HitCooldown);
        }
    }
}

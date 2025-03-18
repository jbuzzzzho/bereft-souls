using Terraria;

namespace PackBuilder.Common.JsonBuilding.Items.Changes
{
    internal class VanillaItemChange : IItemChange
    {
        public string? Damage { get; set; }
        public string? CritRate { get; set; }
        public string? Defense { get; set; }
        public string? HammerPower { get; set; }
        public string? PickaxePower { get; set; }
        public string? AxePower { get; set; }
        public string? Healing { get; set; }
        public string? ManaRestoration { get; set; }
        public string? Knockback { get; set; }
        public string? LifeRegen { get; set; }
        public string? ManaCost { get; set; }
        public string? ShootSpeed { get; set; }
        public string? UseTime { get; set; }

        public void ApplyTo(Item item)
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

            Delta(ref item.damage, Damage);
            Delta(ref item.crit, CritRate);
            Delta(ref item.defense, Defense);
            Delta(ref item.hammer, HammerPower);
            Delta(ref item.pick, PickaxePower);
            Delta(ref item.axe, AxePower);
            Delta(ref item.healLife, Healing);
            Delta(ref item.healMana, ManaRestoration);
            DeltaF(ref item.knockBack, Knockback);
            Delta(ref item.lifeRegen, LifeRegen);
            Delta(ref item.mana, ManaCost);
            DeltaF(ref item.shootSpeed, ShootSpeed);
            Delta(ref item.useTime, UseTime);
        }
    }
}

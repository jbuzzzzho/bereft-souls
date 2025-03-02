using CalamityMod;
using Terraria;
using Terraria.ModLoader;

namespace PackBuilder.Common.JsonBuilding.NPCs.Changes
{
    [JITWhenModsEnabled("CalamityMod")]
    internal class CalamityNPCChange : INPCChange
    {
        public string? DamageReduction { get; set; }

        public void ApplyTo(NPC npc)
        {
            static float Delta(float field, string? adjustment)
            {
                if (adjustment is null)
                    return field;

                if (adjustment.StartsWith('+'))
                    field += float.Parse(adjustment.Substring(1));

                else if (adjustment.StartsWith('-'))
                    field += float.Parse(adjustment);

                else
                    field = float.Parse(adjustment);

                return field;
            }
            static void DeltaF(ref float field, string? adjustment) => field = Delta(field, adjustment);
            static void DeltaI(ref int field, string? adjustment)
            {
                float val = field;
                DeltaF(ref val, adjustment);
                field = (int)val;
            }

            var calNpc = npc.Calamity();

            calNpc.DR = Delta(calNpc.DR, DamageReduction);
        }
    }
}

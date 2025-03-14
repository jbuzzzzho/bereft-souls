using Terraria;

namespace PackBuilder.Common.JsonBuilding.NPCs.Changes
{
    internal class VanillaNpcChange : INPCChange
    {
        public string? Damage { get; set; }
        public string? Defense { get; set; }
        public string? Health { get; set; }
        public string? KnockbackScaling { get; set; }
        public string? NPCSlots { get; set; }

        public void ApplyTo(NPC npc)
        {
            static void DeltaF(ref float field, string? adjustment)
            {
                if (adjustment is null)
                    return;

                if (adjustment.StartsWith('+'))
                    field += float.Parse(adjustment.Substring(1));

                else if (adjustment.StartsWith('-'))
                    field += float.Parse(adjustment);

                else
                    field = float.Parse(adjustment);
            }
            static void Delta(ref int field, string? adjustment)
            {
                float val = field;
                DeltaF(ref val, adjustment);
                field = (int)val;
            }

            Delta(ref npc.damage, Damage);
            npc.defDamage = npc.damage;

            Delta(ref npc.defense, Defense);
            npc.defDefense = npc.defense;

            Delta(ref npc.lifeMax, Health);
            npc.life = npc.lifeMax;

            DeltaF(ref npc.knockBackResist, KnockbackScaling);
            DeltaF(ref npc.npcSlots, NPCSlots);
        }
    }
}

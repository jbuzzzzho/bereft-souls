using Newtonsoft.Json;
using PackBuilder.Common.JsonBuilding.NPCs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PackBuilder.Common.Systems
{
    internal class PackBuilderNPC : GlobalNPC
    {
        public static List<NPCChanges>[] NPCModSets = null;

        public override void SetDefaults(NPC entity) => NPCModSets[entity.type]?.ForEach(c => c.ApplyTo(entity));
    }

    internal class NPCModifier : ModSystem
    {
        public override void PostSetupContent()
        {
            // Collects ALL .npcmod.json files from all mods into a list.
            List<byte[]> jsonEntries = [];

            // Collects the loaded NPC mods to pass to the set factory initialization.
            Dictionary<int, List<NPCChanges>> factorySets = [];

            foreach (Mod mod in ModLoader.Mods)
            {
                // An array of all .npcmod.json files from this specific mod.
                var files = (mod.GetFileNames() ?? []).Where(s => s.EndsWith(".npcmod.json", System.StringComparison.OrdinalIgnoreCase));

                // Adds the byte contents of each file to the list.
                foreach (var file in files)
                    jsonEntries.Add(mod.GetFileBytes(file));
            }

            foreach (var jsonEntry in jsonEntries)
            {
                // Convert the raw bytes into raw text.
                string rawJson = Encoding.Default.GetString(jsonEntry);

                // Decode the json into an NPC mod.
                NPCMod npcMod = JsonConvert.DeserializeObject<NPCMod>(rawJson)!;

                // Get the NPC mod ready for factory initialization.
                foreach (string npc in npcMod.NPCs)
                {
                    int npcType = GetNPC(npc);

                    factorySets.TryAdd(npcType, []);
                    factorySets[npcType].Add(npcMod.Changes);
                }
            }

            // Setup the factory for fast access to NPC lookup.
            object[] factory = factorySets.SelectMany<KeyValuePair<int, List<NPCChanges>>, object>(kvp => [kvp.Key, kvp.Value]).ToArray();
            PackBuilderNPC.NPCModSets = NPCID.Sets.Factory.CreateCustomSet<List<NPCChanges>>(null, inputs: factory);
        }
    }
}

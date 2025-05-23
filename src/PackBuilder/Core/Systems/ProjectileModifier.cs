﻿using Newtonsoft.Json;
using PackBuilder.Common.JsonBuilding.Projectiles;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace PackBuilder.Core.Systems
{
    internal class PackBuilderProjectile : GlobalProjectile
    {
        public static ImmutableDictionary<int, List<ProjectileChanges>>? ProjectileModSets = null;

        public override void SetDefaults(Projectile entity)
        {
            if (ProjectileModSets?.TryGetValue(entity.type, out var value) ?? false)
                value.ForEach(c => c.ApplyTo(entity));
        }
    }

    internal class ProjectileModifier : ModSystem
    {
        public override void PostSetupContent()
        {
            // Collects ALL .projectilemod.json files from all mods into a list.
            List<byte[]> jsonEntries = [];

            // Collects the loaded projectile mods to pass to the set factory initialization.
            Dictionary<int, List<ProjectileChanges>> factorySets = [];

            foreach (Mod mod in ModLoader.Mods)
            {
                // An array of all .projectilemod.json files from this specific mod.
                var files = (mod.GetFileNames() ?? []).Where(s => s.EndsWith(".projectilemod.json", System.StringComparison.OrdinalIgnoreCase));

                // Adds the byte contents of each file to the list.
                foreach (var file in files)
                    jsonEntries.Add(mod.GetFileBytes(file));
            }

            foreach (var jsonEntry in jsonEntries)
            {
                // Convert the raw bytes into raw text.
                string rawJson = Encoding.Default.GetString(jsonEntry);

                // Decode the json into an projectile mod.
                ProjectileMod projectileMod = JsonConvert.DeserializeObject<ProjectileMod>(rawJson)!;

                // Get the projectile mod ready for factory initialization.
                foreach (string projectile in projectileMod.Projectiles)
                {
                    int projectileType = GetProjectile(projectile);

                    factorySets.TryAdd(projectileType, []);
                    factorySets[projectileType].Add(projectileMod.Changes);
                }
            }

            // Setup the factory for fast access to projectile lookup.
            PackBuilderProjectile.ProjectileModSets = factorySets.ToImmutableDictionary();
        }
    }
}

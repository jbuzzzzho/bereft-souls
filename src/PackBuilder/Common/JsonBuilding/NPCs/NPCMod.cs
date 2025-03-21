using System.Collections.Generic;

namespace PackBuilder.Common.JsonBuilding.NPCs
{
    internal class NPCMod
    {
        public List<string> NPCs = [];

        public required NPCChanges Changes { get; set; }
    }
}

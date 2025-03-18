using PackBuilder.Common.JsonBuilding.Items.Changes;
using System.Collections.Generic;
using Terraria;

namespace PackBuilder.Common.JsonBuilding.Items
{
    internal class ItemChanges
    {
        public List<IItemChange> Changes = [];

        public VanillaItemChange Terraria { set => Changes.Add(value); }

        public void ApplyTo(Item item)
        {
            foreach (var change in Changes)
                change.ApplyTo(item);
        }
    }
}

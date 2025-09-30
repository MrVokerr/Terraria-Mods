using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Vokerropweapons.Content.Buffs
{
    public class Voidburn : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // No SetDefault or BuffID.Sets needed in 1.4
            // Localization handles name/description
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen -= 80;
            npc.defense -= 20;
        }
    }
}

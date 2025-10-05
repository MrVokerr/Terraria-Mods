using Terraria;
using Terraria.ModLoader;
using Terraria.ID; // ✅ needed for BuffID.Sets.*

namespace Vokerropweapons.Content.Buffs
{
    public class BeanMinionBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; // hides timer
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Minions.BeanMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            buffName = "Bean Minion";
            tip = "A loyal bean fights for you!";
        }
    }
}

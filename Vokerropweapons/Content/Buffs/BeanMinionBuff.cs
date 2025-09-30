using Terraria;
using Terraria.ModLoader;

namespace Vokerropweapons.Content.Buffs
{
    public class BeanMinionBuff : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;

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
    }
}

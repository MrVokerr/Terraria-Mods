using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace Vokerropweapons.Players
{
    public class VoidreaverPlayer : ModPlayer
    {
        private int timeRipCooldown = 0;

        public override void ResetEffects()
        {
            if (timeRipCooldown > 0)
                timeRipCooldown--;
        }

        public bool CanUseTimeRip() => timeRipCooldown <= 0;

        public void TriggerTimeRip()
        {
            timeRipCooldown = 600; // 10 seconds
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].active && !Main.npc[i].friendly && !Main.npc[i].boss)
                {
                    Main.npc[i].AddBuff(BuffID.Frozen, 120);
                }
            }

            Main.NewText("Time rips as you unleash the Void.", Microsoft.Xna.Framework.Color.Purple);
        }
    }
}

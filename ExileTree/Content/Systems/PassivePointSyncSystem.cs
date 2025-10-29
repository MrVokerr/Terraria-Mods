using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExileTree.Content.Players;

namespace ExileTree.Content.Systems
{
    /// <summary>
    /// Ensures all players receive milestone points when joining a world,
    /// even if they join mid-playthrough or after bosses were defeated.
    /// Only runs on world load - boss detection happens in NPC.OnKill hooks.
    /// </summary>
    public class PassivePointSyncSystem : ModSystem
    {
        public override void OnWorldLoad()
        {
            // Force recalculation for all players on world load
            // This handles server restarts, world reloads, and late-joining players
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player != null && player.active)
                {
                    var modPlayer = player.GetModPlayer<ExileTreePlayer>();
                    modPlayer.PerformFullRecalculation(showMessage: false);
                }
            }
        }
    }
}

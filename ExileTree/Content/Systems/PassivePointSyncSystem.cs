using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExileTree.Content.Players;

namespace ExileTree.Content.Systems
{
    /// <summary>
    /// Ensures all players receive milestone points when joining a world,
    /// even if they join mid-playthrough or after bosses were defeated.
    /// </summary>
    public class PassivePointSyncSystem : ModSystem
    {
        private int lastKnownMilestoneCount = 0;
        private int ticksSinceLastCheck = 0;
        private const int CHECK_INTERVAL = 3600; // Check every minute (60 ticks/sec * 60 sec = 3600 ticks)

        public override void OnWorldLoad()
        {
            // Cache the current milestone count when world loads
            lastKnownMilestoneCount = BossMilestones.GetMilestoneCount();
        }

        public override void PostUpdateEverything()
        {
            // Only run on server or singleplayer
            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
                return;

            ticksSinceLastCheck++;
            if (ticksSinceLastCheck < CHECK_INTERVAL)
                return;

            ticksSinceLastCheck = 0;

            // Check if any new bosses were defeated
            int currentMilestones = BossMilestones.GetMilestoneCount();
            if (currentMilestones > lastKnownMilestoneCount)
            {
                // New boss was defeated - sync all players
                int newPoints = currentMilestones - lastKnownMilestoneCount;
                lastKnownMilestoneCount = currentMilestones;

                // Grant points to all connected players
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player != null && player.active)
                    {
                        var modPlayer = player.GetModPlayer<ExileTreePlayer>();
                        
                        // Only grant if they haven't already received these points
                        int delta = currentMilestones - modPlayer.milestonePointsGranted;
                        if (delta > 0)
                        {
                            modPlayer.skillPoints += delta;
                            modPlayer.milestonePointsGranted = currentMilestones;
                            
                            if (Main.netMode != Terraria.ID.NetmodeID.Server)
                            {
                                Main.NewText(
                                    $"[Exile Tree] Boss defeated! +{delta} passive point{(delta == 1 ? "" : "s")}!",
                                    Color.Gold
                                );
                            }
                        }
                    }
                }
            }
        }
    }
}

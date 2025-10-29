using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameInput;
using Terraria.Chat;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using ExileTree.Content.Systems;
using Terraria.Localization;

namespace ExileTree.Content.Players
{
    /// <summary>
    /// Player data for the Exile Tree passive skill system.
    /// 
    /// SIMPLIFIED FAILSAFE SYSTEM FOR PASSIVE POINTS:
    /// This system ensures players always have the correct number of passive points
    /// by recalculating from world state only when necessary:
    /// 
    /// WHEN POINTS ARE SYNCED:
    /// 1. WORLD ENTRY: OnEnterWorld() recalculates points from boss state (primary sync point)
    /// 2. FIRST TICK: PostUpdate() backup recalc on first tick (multiplayer safety net)
    /// 3. WORLD LOAD: PassivePointSyncSystem recalcs all players on server start/reload
    /// 4. COMMAND: /passives command auto-syncs and displays point status
    /// 5. DATA LOAD: LoadData() corruption detection validates save data
    /// 
    /// NO PERIODIC CHECKING - Zero tick-by-tick overhead. Boss detection happens naturally
    /// through OnEnterWorld (when you see new bosses are dead) and world load events
    /// </summary>
    public class ExileTreePlayer : ModPlayer
    {
        // Changed from HashSet to Dictionary to track rank per node
        public Dictionary<string, int> nodeRanks = new();
        public int skillPoints = 2;

        // How many milestone boss points this player has ALREADY been granted in this world
        public int milestonePointsGranted = 0;

        // Helper property for backwards compatibility
        public HashSet<string> unlockedNodes => new HashSet<string>(nodeRanks.Keys);

        // --------------------------------------------------
        // Initialization and Save / Load
        // --------------------------------------------------
        public override void Initialize()
        {
            nodeRanks.Clear();
            skillPoints = 2;
            milestonePointsGranted = 0;
            hasInitialSyncOccurred = false;
        }

        public override void SaveData(TagCompound tag)
        {
            // Save as two lists: node IDs and their ranks
            var nodeIds = new List<string>();
            var ranks = new List<int>();
            foreach (var kvp in nodeRanks)
            {
                nodeIds.Add(kvp.Key);
                ranks.Add(kvp.Value);
            }
            tag["nodeIds"] = nodeIds;
            tag["nodeRanks"] = ranks;
            tag["skillPoints"] = skillPoints;
            tag["milestonePointsGranted"] = milestonePointsGranted;
        }

        public override void LoadData(TagCompound tag)
        {
            nodeRanks.Clear();
            var nodeIds = tag.GetList<string>("nodeIds");
            var ranks = tag.GetList<int>("nodeRanks");
            for (int i = 0; i < nodeIds.Count && i < ranks.Count; i++)
            {
                nodeRanks[nodeIds[i]] = ranks[i];
            }
            skillPoints = tag.GetInt("skillPoints");
            milestonePointsGranted = tag.GetInt("milestonePointsGranted");
            
            // CRITICAL: Don't trust save data AT ALL. Just flag it for recalculation.
            // This handles: legacy bugs, world transfers, character transfers, corruption, cheating
            // We'll recalculate from world state on entry anyway, so these values are just placeholders
            bool suspiciousData = false;
            
            if (skillPoints < 0 || skillPoints > 19) // Max possible: 2 base + 17 bosses
            {
                suspiciousData = true;
            }
            
            if (milestonePointsGranted < 0 || milestonePointsGranted > 17)
            {
                suspiciousData = true;
            }
            
            if (suspiciousData)
            {
                Main.NewText("[Exile Tree] Save data will be validated against world state.", Color.Yellow);
            }
            
            hasInitialSyncOccurred = false; // Always recalc on world entry
        }

        // --------------------------------------------------
        // World Entry + Passive Point Sync
        // --------------------------------------------------
        private bool hasInitialSyncOccurred = false;

        public override void OnEnterWorld()
        {
            // Force initial sync when entering world - this is the PRIMARY sync point
            hasInitialSyncOccurred = false;
            PerformFullRecalculation(showMessage: false);
        }

        public override void PostUpdate()
        {
            // ONLY run once on first tick as backup for multiplayer clients
            // After that, do nothing - let the NPC.OnKill hook handle boss detection
            if (!hasInitialSyncOccurred)
            {
                PerformFullRecalculation(showMessage: false);
            }
        }

        /// <summary>
        /// BULLETPROOF RECALCULATION:
        /// Completely recalculates what the player SHOULD have based on world state,
        /// compares to what they DO have, and fixes any discrepancies.
        /// Handles: new characters, old characters, version updates, world transfers, 
        /// character transfers, save corruption, cheating, multiplayer joins.
        /// </summary>
        public void PerformFullRecalculation(bool showMessage)
        {
            // 1. Calculate how many points they SHOULD have
            int bossesDefeated = BossMilestones.GetMilestoneCount();
            int expectedBossPoints = bossesDefeated;
            int expectedTotalPoints = 2 + expectedBossPoints; // 2 base + boss points

            // 2. Calculate how many points they've SPENT
            int pointsSpent = 0;
            foreach (var kvp in nodeRanks)
            {
                if (PassiveTreeSystem.AllNodes.TryGetValue(kvp.Key, out var node))
                {
                    pointsSpent += kvp.Value * node.SkillPointCost;
                }
            }

            // 3. CRITICAL CHECK: Do they have more nodes than possible?
            // This catches: world transfers, character transfers, save editing, corruption
            if (pointsSpent > expectedTotalPoints)
            {
                // They've spent MORE points than they should have total
                // This is IMPOSSIBLE in a legitimate scenario
                // Force a full respec to prevent cheating/corruption
                nodeRanks.Clear();
                pointsSpent = 0;
                
                if (showMessage && Main.netMode != Terraria.ID.NetmodeID.Server)
                {
                    Main.NewText(
                        "[Exile Tree] Detected impossible node allocation. Tree has been reset. This can happen when transferring characters between worlds.",
                        Color.Red
                    );
                }
            }

            // 4. Calculate what they SHOULD have available
            int expectedAvailable = expectedTotalPoints - pointsSpent;

            // 5. Compare to what they ACTUALLY have
            int actualAvailable = skillPoints;
            int difference = expectedAvailable - actualAvailable;

            // 6. Fix any discrepancy
            if (difference != 0)
            {
                skillPoints = expectedAvailable;
                milestonePointsGranted = bossesDefeated;

                if (showMessage && Main.netMode != Terraria.ID.NetmodeID.Server)
                {
                    if (difference > 0)
                    {
                        Main.NewText(
                            $"[Exile Tree] Corrected your passive points! Added {difference} missing point{(difference == 1 ? "" : "s")}.",
                            Color.LimeGreen
                        );
                    }
                    else
                    {
                        Main.NewText(
                            $"[Exile Tree] Corrected your passive points! Removed {-difference} excess point{(difference == -1 ? "" : "s")}.",
                            Color.Yellow
                        );
                    }
                }
            }
            else
            {
                // No correction needed, but still update milestone tracker
                milestonePointsGranted = bossesDefeated;
            }

            hasInitialSyncOccurred = true;
        }

        // --------------------------------------------------
        // Passive Effects + Respec
        // --------------------------------------------------
        public override void ResetEffects()
        {
            // Apply every unlocked node's effect based on its rank
            foreach (var kvp in nodeRanks)
            {
                string nodeId = kvp.Key;
                int rank = kvp.Value;
                
                if (PassiveTreeSystem.AllNodes.TryGetValue(nodeId, out var node))
                {
                    // Apply effect for each rank
                    for (int i = 0; i < rank; i++)
                    {
                        PassiveTreeSystem.ApplyNodeEffect(this, node.ID);
                    }
                }
            }
        }

        public void AddSkillPoint(int amount = 1)
        {
            skillPoints += amount;
            if (skillPoints < 0) skillPoints = 0;
        }

        public void RespecAll()
        {
            nodeRanks.Clear();
            PerformFullRecalculation(showMessage: true);
        }

        // --------------------------------------------------
        // UI Keybind
        // --------------------------------------------------
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (ExileTree.ToggleTreeKeybind != null && ExileTree.ToggleTreeKeybind.JustPressed)
            {
                ExileTreeUISystem.Toggle();
            }
        }
    }

    // --------------------------------------------------
    // Command: /passives — check milestone status and point breakdown + auto-sync
    // --------------------------------------------------
    public class PassivesCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "passives";
        public override string Usage => "/passives";
        public override string Description => "Check your passive point status, defeated bosses, and auto-sync if needed.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = caller.Player;
            var modPlayer = player.GetModPlayer<ExileTreePlayer>();

            // AUTO-SYNC ALL PLAYERS: If on server or singleplayer, sync everyone
            if (Main.netMode != Terraria.ID.NetmodeID.MultiplayerClient)
            {
                // Sync all connected players
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (p != null && p.active)
                    {
                        var mp = p.GetModPlayer<ExileTreePlayer>();
                        mp.PerformFullRecalculation(showMessage: false);
                    }
                }
                
                Main.NewText("[Exile Tree] All players synced!", Color.Gold);
            }
            else
            {
                // Client-only: just sync yourself
                modPlayer.PerformFullRecalculation(showMessage: false);
            }

            // Get updated values after sync
            int bossesDefeated = BossMilestones.GetMilestoneCount();
            int pointsFromBosses = modPlayer.milestonePointsGranted;
            int availablePoints = modPlayer.skillPoints;
            
            // Calculate spent points
            int pointsSpent = 0;
            foreach (var kvp in modPlayer.nodeRanks)
            {
                if (PassiveTreeSystem.AllNodes.TryGetValue(kvp.Key, out var node))
                {
                    pointsSpent += kvp.Value * node.SkillPointCost;
                }
            }

            // Calculate max possible points based on progression
            int maxPossiblePoints = 2; // Base points
            
            // Pre-Hardmode bosses (7 total)
            maxPossiblePoints += 7;
            
            // Hardmode bosses (10 total) - only count if in hardmode
            if (Main.hardMode)
            {
                maxPossiblePoints += 10;
            }
            
            int missingPoints = maxPossiblePoints - (2 + pointsFromBosses);
            
            // Display summary
            Main.NewText("=== Exile Tree Passive Points ===", Color.Gold);
            Main.NewText("Base Points: 2", Color.LightGreen);
            Main.NewText("Boss Milestones Granted: " + pointsFromBosses.ToString(), Color.LightGreen);
            Main.NewText("Total Earned: " + (2 + pointsFromBosses).ToString(), Color.Cyan);
            Main.NewText("Points Spent: " + pointsSpent.ToString(), Color.Orange);
            Main.NewText("Available Points: " + availablePoints.ToString() + " / " + maxPossiblePoints.ToString() + " (Missing: " + missingPoints.ToString() + ")", Color.Yellow);
            
            // Build list of defeated boss names
            var defeatedBosses = new System.Collections.Generic.List<string>();
            if (NPC.downedSlimeKing) defeatedBosses.Add("King Slime");
            if (NPC.downedBoss1) defeatedBosses.Add("Eye of Cthulhu");
            if (NPC.downedBoss2) defeatedBosses.Add("Evil Boss");
            if (NPC.downedQueenBee) defeatedBosses.Add("Queen Bee");
            if (NPC.downedBoss3) defeatedBosses.Add("Skeletron");
            if (NPC.downedDeerclops) defeatedBosses.Add("Deerclops");
            if (Main.hardMode) defeatedBosses.Add("Wall of Flesh");
            if (NPC.downedQueenSlime) defeatedBosses.Add("Queen Slime");
            if (NPC.downedMechBoss1) defeatedBosses.Add("The Twins");
            if (NPC.downedMechBoss2) defeatedBosses.Add("The Destroyer");
            if (NPC.downedMechBoss3) defeatedBosses.Add("Skeletron Prime");
            if (NPC.downedPlantBoss) defeatedBosses.Add("Plantera");
            if (NPC.downedGolemBoss) defeatedBosses.Add("Golem");
            if (NPC.downedEmpressOfLight) defeatedBosses.Add("Empress of Light");
            if (NPC.downedFishron) defeatedBosses.Add("Duke Fishron");
            if (NPC.downedAncientCultist) defeatedBosses.Add("Lunatic Cultist");
            if (NPC.downedMoonlord) defeatedBosses.Add("Moon Lord");
            
            // Display defeated bosses in one line
            if (defeatedBosses.Count > 0)
            {
                string bossNames = string.Join(", ", defeatedBosses);
                Main.NewText("Milestone Bosses Killed: " + bossNames, Color.LimeGreen);
            }
            else
            {
                Main.NewText("Milestone Bosses Killed: None", Color.Gray);
            }
        }
    }
}

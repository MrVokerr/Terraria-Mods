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
    /// FAILSAFE SYSTEM FOR PASSIVE POINTS:
    /// This system ensures players always receive the correct number of passive points,
    /// regardless of when they join or how they play:
    /// 
    /// 1. NEW CHARACTER: Starts with 2 base points + points from any bosses already defeated in the world
    /// 2. EXISTING CHARACTER: On world entry, automatically grants missing points from newly defeated bosses
    /// 3. MULTIPLAYER JOIN: Players joining mid-playthrough get all points from bosses defeated before they joined
    /// 4. MOD ADDED MID-PLAYTHROUGH: Automatically calculates and grants points from all previously defeated bosses
    /// 5. LIVE BOSS DEFEATS: PassivePointSyncSystem detects new boss kills and grants points to all online players
    /// 6. CORRUPTED SAVE RECOVERY: /recalcpassives command allows manual recalculation if needed
    /// 
    /// The system uses three layers of sync:
    /// - OnEnterWorld(): Initial grant when entering world
    /// - PostUpdate(): Backup sync on first tick (catches multiplayer edge cases)
    /// - PassivePointSyncSystem: Server-side periodic check for new boss defeats
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
            
            // Load milestonePointsGranted with backwards compatibility
            if (tag.ContainsKey("milestonePointsGranted"))
            {
                milestonePointsGranted = tag.GetInt("milestonePointsGranted");
            }
            else
            {
                // Old save without milestone tracking - assume they already have all boss points
                // We calculate this by: (current skillPoints - 2 base points - points spent on nodes)
                int pointsSpent = 0;
                foreach (var kvp in nodeRanks)
                {
                    if (PassiveTreeSystem.AllNodes.TryGetValue(kvp.Key, out var node))
                    {
                        pointsSpent += kvp.Value * node.SkillPointCost;
                    }
                }
                
                // Assume milestone points = current + spent - 2 base
                // This prevents granting duplicate points when updating from old version
                int totalPointsEverHad = skillPoints + pointsSpent;
                milestonePointsGranted = System.Math.Max(0, totalPointsEverHad - 2);
            }
            
            hasInitialSyncOccurred = false; // Reset sync flag on load
        }

        // --------------------------------------------------
        // World Entry + Passive Point Sync
        // --------------------------------------------------
        private bool hasInitialSyncOccurred = false;

        public override void OnEnterWorld()
        {
            // Force initial sync when entering world
            hasInitialSyncOccurred = false;
            PerformMilestoneSync(isInitialSync: true);
        }

        public override void PostUpdate()
        {
            // Perform initial sync on first update if it hasn't happened yet
            // This ensures multiplayer clients get synced even if OnEnterWorld was missed
            if (!hasInitialSyncOccurred)
            {
                PerformMilestoneSync(isInitialSync: true);
            }
            else
            {
                // After initial sync, only check for new bosses killed during gameplay
                SyncBossMilestonePoints();
            }
        }

        private void PerformMilestoneSync(bool isInitialSync)
        {
            int currentMilestones = BossMilestones.GetMilestoneCount();
            int delta = currentMilestones - milestonePointsGranted;

            if (delta > 0)
            {
                skillPoints += delta;
                milestonePointsGranted = currentMilestones;

                if (isInitialSync)
                {
                    Main.NewText(
                        $"[Exile Tree] Granted {delta} passive point{(delta == 1 ? "" : "s")} from defeated bosses ({milestonePointsGranted} total).",
                        Color.LimeGreen
                    );
                }
                else
                {
                    Main.NewText(
                        $"[Exile Tree] +{delta} passive point{(delta == 1 ? "" : "s")} from boss milestone{(delta == 1 ? "" : "s")}!",
                        Color.Goldenrod
                    );
                }
            }

            hasInitialSyncOccurred = true;
        }

        private void SyncBossMilestonePoints()
        {
            int defeated = BossMilestones.GetMilestoneCount();
            int delta = defeated - milestonePointsGranted;

            if (delta > 0)
            {
                skillPoints += delta;
                milestonePointsGranted = defeated;
                Main.NewText(
                    $"[Exile Tree] +{delta} passive point{(delta == 1 ? "" : "s")} from boss milestone{(delta == 1 ? "" : "s")}!",
                    Color.Goldenrod
                );
            }
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
            // Calculate total points spent
            int totalSpent = 0;
            foreach (var kvp in nodeRanks)
            {
                if (PassiveTreeSystem.AllNodes.TryGetValue(kvp.Key, out var node))
                {
                    totalSpent += kvp.Value * node.SkillPointCost;
                }
            }
            
            // Refund all spent points
            skillPoints += totalSpent;
            nodeRanks.Clear();
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
    // Command: /passives — check milestone status and point breakdown
    // --------------------------------------------------
    public class PassivesCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "passives";
        public override string Usage => "/passives";
        public override string Description => "Check your passive point status and defeated bosses.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = caller.Player;
            var modPlayer = player.GetModPlayer<ExileTreePlayer>();

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

            // Display summary
            Main.NewText("=== Exile Tree Passive Points ===", Color.Gold);
            Main.NewText($"Base Points: 2", Color.LightGreen);
            Main.NewText($"Boss Milestones Granted: {pointsFromBosses}", Color.LightGreen);
            Main.NewText($"Total Earned: {2 + pointsFromBosses}", Color.Cyan);
            Main.NewText($"Points Spent: {pointsSpent}", Color.Orange);
            Main.NewText($"Available Points: {availablePoints}", Color.Yellow);
            Main.NewText("", Color.White);

            // Display boss milestone details
            Main.NewText("=== World Boss Milestones ===", Color.Gold);
            Main.NewText($"Pre-Hardmode Bosses:", Color.Cyan);
            Main.NewText($"  {(NPC.downedSlimeKing ? "✓" : "✗")} King Slime", NPC.downedSlimeKing ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedBoss1 ? "✓" : "✗")} Eye of Cthulhu", NPC.downedBoss1 ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedBoss2 ? "✓" : "✗")} Evil Boss (EoW/BoC)", NPC.downedBoss2 ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedQueenBee ? "✓" : "✗")} Queen Bee", NPC.downedQueenBee ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedBoss3 ? "✓" : "✗")} Skeletron", NPC.downedBoss3 ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedDeerclops ? "✓" : "✗")} Deerclops", NPC.downedDeerclops ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(Main.hardMode ? "✓" : "✗")} Wall of Flesh", Main.hardMode ? Color.LimeGreen : Color.Gray);

            Main.NewText($"Hardmode Bosses:", Color.Cyan);
            Main.NewText($"  {(NPC.downedQueenSlime ? "✓" : "✗")} Queen Slime", NPC.downedQueenSlime ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedMechBoss1 ? "✓" : "✗")} The Twins", NPC.downedMechBoss1 ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedMechBoss2 ? "✓" : "✗")} The Destroyer", NPC.downedMechBoss2 ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedMechBoss3 ? "✓" : "✗")} Skeletron Prime", NPC.downedMechBoss3 ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedPlantBoss ? "✓" : "✗")} Plantera", NPC.downedPlantBoss ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedGolemBoss ? "✓" : "✗")} Golem", NPC.downedGolemBoss ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedEmpressOfLight ? "✓" : "✗")} Empress of Light", NPC.downedEmpressOfLight ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedFishron ? "✓" : "✗")} Duke Fishron", NPC.downedFishron ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedAncientCultist ? "✓" : "✗")} Lunatic Cultist", NPC.downedAncientCultist ? Color.LimeGreen : Color.Gray);
            Main.NewText($"  {(NPC.downedMoonlord ? "✓" : "✗")} Moon Lord", NPC.downedMoonlord ? Color.LimeGreen : Color.Gray);

            Main.NewText("", Color.White);
            Main.NewText($"Total Bosses Defeated: {bossesDefeated}/17", Color.Gold);
            
            // Warning if desync detected
            if (bossesDefeated != pointsFromBosses)
            {
                Main.NewText($"⚠ Warning: Mismatch detected! Use /recalcpassives to sync.", Color.Red);
            }
        }
    }

    // --------------------------------------------------
    // Command: /recalcpassives — manually re-sync milestone points
    // --------------------------------------------------
    public class RecalcPassivesCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "recalcpassives";
        public override string Usage => "/recalcpassives";
        public override string Description => "Recalculate passive points from previously defeated bosses.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = caller.Player;
            var modPlayer = player.GetModPlayer<ExileTreePlayer>();

            int beforeMilestone = modPlayer.milestonePointsGranted;
            int defeated = BossMilestones.GetMilestoneCount();
            int gained = defeated - beforeMilestone;

            if (gained > 0)
            {
                modPlayer.skillPoints += gained;
                modPlayer.milestonePointsGranted = defeated;

                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"[Exile Tree] Recalculated milestones → +{gained} passive point{(gained == 1 ? "" : "s")} granted. Total: {modPlayer.skillPoints} available."
                ), Color.LimeGreen);
            }
            else if (gained < 0)
            {
                // Edge case: milestonePointsGranted is somehow higher than actual (corrupted save?)
                // Reset to actual count without adding/removing points
                modPlayer.milestonePointsGranted = defeated;
                
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"[Exile Tree] Milestone count corrected to {defeated}. No points changed."
                ), Color.Yellow);
            }
            else
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"[Exile Tree] All milestone points are already accounted for ({modPlayer.milestonePointsGranted} bosses defeated). You have {modPlayer.skillPoints} points available."
                ), Color.Gray);
            }
        }
    }
}

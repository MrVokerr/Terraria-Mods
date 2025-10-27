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
    public class ExileTreePlayer : ModPlayer
    {
        public HashSet<string> unlockedNodes = new();
        public int skillPoints = 2;

        // How many milestone boss points this player has ALREADY been granted in this world
        public int milestonePointsGranted = 0;

        // --------------------------------------------------
        // Initialization and Save / Load
        // --------------------------------------------------
        public override void Initialize()
        {
            unlockedNodes.Clear();
            skillPoints = 2;
            milestonePointsGranted = 0;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["unlockedNodes"] = new List<string>(unlockedNodes);
            tag["skillPoints"] = skillPoints;
            tag["milestonePointsGranted"] = milestonePointsGranted;
        }

        public override void LoadData(TagCompound tag)
        {
            unlockedNodes = new HashSet<string>(tag.GetList<string>("unlockedNodes"));
            skillPoints = tag.GetInt("skillPoints");
            milestonePointsGranted = tag.GetInt("milestonePointsGranted");
        }

        // --------------------------------------------------
        // World Entry + Passive Point Sync
        // --------------------------------------------------
        public override void OnEnterWorld()
        {
            SyncBossMilestonePoints();
        }

        public override void PostUpdate()
        {
            // Keep milestone sync current in case bosses die during play
            SyncBossMilestonePoints();
        }

        private void SyncBossMilestonePoints()
        {
            int defeated = BossMilestones.GetMilestoneCount();
            int delta = defeated - milestonePointsGranted;

            if (delta > 0)
            {
                skillPoints += delta;
                milestonePointsGranted += delta;
                Main.NewText($"Exile Tree: +{delta} passive point{(delta == 1 ? "" : "s")} from boss milestones (Total granted: {milestonePointsGranted}).", 100, 255, 100);
            }
        }

        // --------------------------------------------------
        // Passive Effects + Respec
        // --------------------------------------------------
        public override void ResetEffects()
        {
            // Apply every unlocked node’s effect each tick
            foreach (var id in unlockedNodes)
            {
                if (PassiveTreeSystem.AllNodes.TryGetValue(id, out var node))
                {
                    PassiveTreeSystem.ApplyNodeEffect(this, node.ID);
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
            int spent = unlockedNodes.Count;
            unlockedNodes.Clear();
            AddSkillPoint(spent);
            Main.NewText($"Exile Tree: Refunded {spent} point{(spent == 1 ? "" : "s")}.");
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

            int before = modPlayer.skillPoints;
            int beforeMilestone = modPlayer.milestonePointsGranted;
            int defeated = BossMilestones.GetMilestoneCount();

            int gained = defeated - beforeMilestone;
            if (gained > 0)
            {
                modPlayer.skillPoints += gained;
                modPlayer.milestonePointsGranted += gained;

                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Exile Tree: Recalculated milestones → +{gained} new passive point{(gained == 1 ? "" : "s")}."
                ), Color.LimeGreen);
            }
            else
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Exile Tree: No new milestones to add (Total granted: {modPlayer.milestonePointsGranted})."
                ), Color.Gray);
            }
        }
    }
}
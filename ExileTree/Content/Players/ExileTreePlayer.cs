using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameInput;
using System.Collections.Generic;
using ExileTree.Content.Systems;


namespace ExileTree.Content.Players
{
    public class ExileTreePlayer : ModPlayer
    {
        public HashSet<string> unlockedNodes = new();
        public int skillPoints = 2;

        // How many milestone boss points this player has ALREADY been given in this world
        private int milestonePointsGranted = 0;

        public override void Initialize() {
            unlockedNodes.Clear();
            skillPoints = 2;
            milestonePointsGranted = 0;
        }

        public override void SaveData(TagCompound tag) {
            tag["unlockedNodes"] = new List<string>(unlockedNodes);
            tag["skillPoints"] = skillPoints;
            tag["milestonePointsGranted"] = milestonePointsGranted;
        }

        public override void LoadData(TagCompound tag) {
            unlockedNodes = new HashSet<string>(tag.GetList<string>("unlockedNodes"));
            skillPoints = tag.GetInt("skillPoints");
            milestonePointsGranted = tag.GetInt("milestonePointsGranted");
        }

        public override void OnEnterWorld() {
            SyncBossMilestonePoints();
        }

        public override void PostUpdate() {
            // Keep it in sync during play (covers fresh kills)
            SyncBossMilestonePoints();
        }

        private void SyncBossMilestonePoints() {
            int defeated = BossMilestones.GetMilestoneCount();
            int delta = defeated - milestonePointsGranted;
            if (delta > 0) {
                skillPoints += delta;
                milestonePointsGranted += delta;
                Main.NewText($"Exile Tree: +{delta} passive point(s) from boss milestones (Total granted: {milestonePointsGranted}).");
            }
        }

        public override void ResetEffects() {
            // Apply every unlocked node through the central effect function
            foreach (var id in unlockedNodes) {
                if (PassiveTreeSystem.AllNodes.TryGetValue(id, out var node)) {
                    PassiveTreeSystem.ApplyNodeEffect(Player, node);
                }
            }
        }

        public void AddSkillPoint(int amount = 1) {
            skillPoints += amount;
            if (skillPoints < 0) skillPoints = 0;
        }

        public void RespecAll() {
            int spent = unlockedNodes.Count;
            unlockedNodes.Clear();
            AddSkillPoint(spent);
            Main.NewText($"Exile Tree: Refunded {spent} points.");
        }

public override void ProcessTriggers(TriggersSet triggersSet) {
    if (ExileTree.ToggleTreeKeybind != null && ExileTree.ToggleTreeKeybind.JustPressed) {
        ExileTreeUISystem.Toggle();
    }
}

    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ExileTree.Content.Players;

namespace ExileTree.Content.Systems
{
    // ---- Spacing & layout helpers (affects the whole tree) ----
    public static class Layout
    {
        public const float SPACING_X = 100f;  // Closer spacing between vertical trees
        public const float SPACING_Y = 60f;   // Reduced spacing for smaller screens
        public static readonly Vector2 ORIGIN = new Vector2(400, 10);  // Minimal top padding

        public static Vector2 Grid(float gx, float gy)
            => ORIGIN + new Vector2(gx * SPACING_X, gy * SPACING_Y);
    }

    // ---- Centralized stat identifiers ----
    public static class StatTypes
    {
        public const string Life = "Life";
        public const string Defense = "Defense";
        public const string DamageReduction = "DamageReduction";
        public const string LifeRegen = "LifeRegen";
        public const string MeleeDamage = "MeleeDamage";
        public const string MeleeSpeed = "MeleeSpeed";
        public const string SummonDamage = "SummonDamage";
        public const string SummonSlot = "SummonSlot";
        public const string SentrySlot = "SentrySlot";
        public const string SentryDamage = "SentryDamage";
        public const string MagicDamage = "MagicDamage";
        public const string MagicCrit = "MagicCrit";
        public const string ManaCost = "ManaCost";
        public const string ManaBonus = "ManaBonus";
        public const string RangedDamage = "RangedDamage";
        public const string RangedCrit = "RangedCrit";
        public const string MoveSpeed = "MoveSpeed";
        public const string CritChance = "CritChance";
    }

    // ----------------------------------------------------------------------
    //  Passive Tree System
    // ----------------------------------------------------------------------
    public class PassiveTreeSystem : ModSystem
    {
        // Node definition
        public class PassiveNode
        {
            public string ID;
            public string Name;
            public string Description;
            public Vector2 Position;
            public List<string> ConnectedNodes;
            public string StatType;
            public float Value;
            public bool IsMajor;
            public string IconPath;
            public int SkillPointCost = 1;      // How many points to unlock (default 1, capstones 2)
            public int MaxRank = 1;              // How many times you can invest (1 = single unlock, 3+ = multiple ranks)
            public float ValuePerRank;           // Value gained per rank (for multi-rank nodes)
        }

        public static Dictionary<string, PassiveNode> AllNodes = new();
        public static string StarterNodeId = "Node_MaxHP";

        public override void OnWorldLoad() => LoadTree();
        public override void OnWorldUnload() => AllNodes.Clear();

        // ------------------------------------------------------------------
        // LoadTree — defines the entire passive tree
        // ------------------------------------------------------------------
        public static void LoadTree()
        {
            AllNodes.Clear();

// --------------------------------------------------
// STARTER
// --------------------------------------------------
            // Central Starting Node
            AllNodes["Node_MaxHP"] = new PassiveNode {
    ID = "Node_MaxHP",
    Name = "Guiding Star",
    Description = "+10 Maximum Life, +2 Defense, +5% Movement Speed",
    Position = Layout.Grid(0f, 0f),  // Center top position
    ConnectedNodes = new List<string> { 
        "Node_Melee_1",   // Combat tree
        "Node_Magic_1",   // Magic tree 
        "Node_DR_1",      // Health/Defense tree
        "Node_Ranged_1",  // Ranged tree
        "Node_Summon_1"   // Summoning tree
    },
    StatType = StatTypes.Life,
    Value = 10f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/GuidingStar"
};

// --------------------------------------------------
// ⚔️ COMBAT TREE (Leftmost - Column 1)
// --------------------------------------------------

// Tier 1
AllNodes["Node_Melee_1"] = new PassiveNode {
    ID = "Node_Melee_1",
    Name = "Brutal Force",
    Description = "+5% Melee Damage per rank (Max 3)",
    Position = Layout.Grid(-2f, 1f),
    ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Melee_3" },
    StatType = StatTypes.MeleeDamage,
    Value = 0.05f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/BrutalForce",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 0.05f
};

// Tier 2
AllNodes["Node_Melee_3"] = new PassiveNode {
    ID = "Node_Melee_3",
    Name = "Sharpness",
    Description = "+5% Melee Critical Strike Chance per rank (Max 3)",
    Position = Layout.Grid(-2f, 2f),
    ConnectedNodes = new List<string> { "Node_Melee_1", "Node_Melee_2" },
    StatType = StatTypes.CritChance,
    Value = 5f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/Sharpness",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 5f
};

// Tier 3
AllNodes["Node_Melee_2"] = new PassiveNode {
    ID = "Node_Melee_2",
    Name = "Quick Strikes",
    Description = "+5% Melee Attack Speed per rank (Max 3)",
    Position = Layout.Grid(-2f, 3f),
    ConnectedNodes = new List<string> { "Node_Melee_3", "Node_MeleeMajor_BladedDance", "Node_MeleeMajor_IronVanguard" },
    StatType = StatTypes.MeleeSpeed,
    Value = 0.05f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/QuickStrikes",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 0.05f
};

// Major Node (Tier 4)
AllNodes["Node_MeleeMajor_BladedDance"] = new PassiveNode {
    ID = "Node_MeleeMajor_BladedDance",
    Name = "Blade Dance",
    Description = "Major: +15% Melee Attack Speed, +10% Movement Speed",
    Position = Layout.Grid(-2f, 4f),
    ConnectedNodes = new List<string> { "Node_Melee_2" },
    StatType = StatTypes.MeleeSpeed,
    Value = 0.15f,
    IsMajor = true,
    IconPath = "ExileTree/Content/Assets/Passives/BladeDance",
    SkillPointCost = 2,
    MaxRank = 1,
    ValuePerRank = 0f
};

// Major Node (Tier 5)
AllNodes["Node_MeleeMajor_IronVanguard"] = new PassiveNode {
    ID = "Node_MeleeMajor_IronVanguard",
    Name = "Iron Vanguard",
    Description = "Major: +10% Melee Damage, +25 Defense, -10% Movement Speed",
    Position = Layout.Grid(-2f, 5.17f),
    ConnectedNodes = new List<string> { "Node_Melee_2" },
    StatType = StatTypes.MeleeDamage,
    Value = 0.10f,
    IsMajor = true,
    IconPath = "ExileTree/Content/Assets/Passives/IronVanguard",
    SkillPointCost = 2,
    MaxRank = 1,
    ValuePerRank = 0f
};

// --------------------------------------------------
// 🕷️ SUMMONING TREE (Rightmost - Column 1)
// --------------------------------------------------

// Tier 1
AllNodes["Node_Summon_1"] = new PassiveNode {
    ID = "Node_Summon_1",
    Name = "Spirit Bond",
    Description = "+5% Summon Damage per rank (Max 3)",
    Position = Layout.Grid(2f, 1f),
    ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Summon_2" },
    StatType = StatTypes.SummonDamage,
    Value = 0.05f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/SpiritBond",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 0.05f
};

// Tier 2
AllNodes["Node_Summon_2"] = new PassiveNode {
    ID = "Node_Summon_2",
    Name = "Soul Whisper",
    Description = "+5% Summon Critical Strike Chance per rank (Max 3)",
    Position = Layout.Grid(2f, 2f),
    ConnectedNodes = new List<string> { "Node_Summon_1", "Node_Summon_3" },
    StatType = StatTypes.SummonDamage,
    Value = 5f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/SoulWhisper",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 5f
};

// Tier 3
AllNodes["Node_Summon_3"] = new PassiveNode {
    ID = "Node_Summon_3",
    Name = "Legion Commander",
    Description = "+1 Sentry Slot",
    Position = Layout.Grid(2f, 3f),
    ConnectedNodes = new List<string> { "Node_Summon_2", "Node_SummonMajor_SpiritAscendant" },
    StatType = StatTypes.SentrySlot,
    Value = 1f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/LegionCommander"
};

// Major Node (Bottom)
AllNodes["Node_SummonMajor_SpiritAscendant"] = new PassiveNode {
    ID = "Node_SummonMajor_SpiritAscendant",
    Name = "Spirit Ascendant",
    Description = "Major: +25% Summon Damage while at Max Minions",
    Position = Layout.Grid(2f, 4f),
    ConnectedNodes = new List<string> { "Node_Summon_3", "Node_SummonMajor_DominionOfSouls" },
    StatType = StatTypes.SummonDamage,
    Value = 0.25f,
    IsMajor = true,
    IconPath = "ExileTree/Content/Assets/Passives/SpiritAscendant",
    SkillPointCost = 2,
    MaxRank = 1,
    ValuePerRank = 0f
};

AllNodes["Node_SummonMajor_DominionOfSouls"] = new PassiveNode {
    ID = "Node_SummonMajor_DominionOfSouls",
    Name = "Dominion of Souls",
    Description = "Major: +15% Summon Damage, +3 Minion Slots",
    Position = Layout.Grid(2f, 5.17f),
    ConnectedNodes = new List<string> { "Node_SummonMajor_SpiritAscendant" },
    StatType = StatTypes.SummonDamage,
    Value = 0.15f,
    IsMajor = true,
    IconPath = "ExileTree/Content/Assets/Passives/DominionOfSouls",
    SkillPointCost = 2,
    MaxRank = 1,
    ValuePerRank = 0f
};

// --------------------------------------------------
// ✨ MAGIC TREE (Center-Left - Column 2)
// --------------------------------------------------

// Tier 1
AllNodes["Node_Magic_1"] = new PassiveNode {
    ID = "Node_Magic_1",
    Name = "Arcane Spark",
    Description = "+5% Magic Damage per rank (Max 3)",
    Position = Layout.Grid(-1f, 1f),
    ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Magic_2" },
    StatType = StatTypes.MagicDamage,
    Value = 0.05f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/ArcaneSpark",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 0.05f
};

// Tier 2
AllNodes["Node_Magic_2"] = new PassiveNode {
    ID = "Node_Magic_2",
    Name = "Arcane Focus",
    Description = "+5% Magic Critical Strike Chance per rank (Max 3)",
    Position = Layout.Grid(-1f, 2f),
    ConnectedNodes = new List<string> { "Node_Magic_1", "Node_Magic_4" },
    StatType = StatTypes.MagicCrit,
    Value = 5f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/ArcaneFocus",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 5f
};

// Tier 3
AllNodes["Node_Magic_4"] = new PassiveNode {
    ID = "Node_Magic_4",
    Name = "Mana Font",
    Description = "+10 Maximum Mana per rank (Max 3)",
    Position = Layout.Grid(-1f, 3f),
    ConnectedNodes = new List<string> { "Node_Magic_2", "Node_Magic_3", "Node_MagicMajor_Spellweaver" },
    StatType = StatTypes.ManaBonus,
    Value = 10f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/ManaFont",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 10f
};

// Major Node (Tier 4)
AllNodes["Node_Magic_3"] = new PassiveNode {
    ID = "Node_Magic_3",
    Name = "Archmage",
    Description = "Major: +15% Magic Damage, +10% Magic Crit, +40 Max Mana",
    Position = Layout.Grid(-1f, 4f),
    ConnectedNodes = new List<string> { "Node_Magic_4" },
    StatType = StatTypes.MagicDamage,
    Value = 0.15f,
    IsMajor = true,
    IconPath = "ExileTree/Content/Assets/Passives/Archmage",
    SkillPointCost = 2,
    MaxRank = 1,
    ValuePerRank = 0f
};

AllNodes["Node_MagicMajor_Spellweaver"] = new PassiveNode {
    ID = "Node_MagicMajor_Spellweaver",
    Name = "Spellweaver",
    Description = "Major: 25% Reduced Mana Cost, +10 Mana Regen/sec",
    Position = Layout.Grid(-1f, 5.17f),
    ConnectedNodes = new List<string> { "Node_Magic_4" },
    StatType = StatTypes.ManaCost,
    Value = 0.25f,
    IsMajor = true,
    IconPath = "ExileTree/Content/Assets/Passives/Spellweaver",
    SkillPointCost = 2,
    MaxRank = 1,
    ValuePerRank = 0f
};

// --------------------------------------------------
// ❤️ HEALTH/DEFENSE TREE (Center - Column 3)
// --------------------------------------------------

// Tier 1
AllNodes["Node_DR_1"] = new PassiveNode {
    ID = "Node_DR_1",
    Name = "Resilience",
    Description = "+5% Damage Reduction per rank (Max 3)",
    Position = Layout.Grid(0f, 1f),
    ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Life_1" },
    StatType = StatTypes.DamageReduction,
    Value = 0.05f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/Resilience",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 0.05f
};

// Tier 2
AllNodes["Node_Life_1"] = new PassiveNode {
    ID = "Node_Life_1",
    Name = "Vital Reserves",
    Description = "+20 Maximum Life",
    Position = Layout.Grid(0f, 2f),
    ConnectedNodes = new List<string> { "Node_DR_1", "Node_Regen_2" },
    StatType = StatTypes.Life,
    Value = 20f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/VitalReserves"
};

// Tier 3
AllNodes["Node_Regen_2"] = new PassiveNode {
    ID = "Node_Regen_2",
    Name = "Mend",
    Description = "+1 Life Regeneration per second per rank (Max 3)",
    Position = Layout.Grid(0f, 3f),
    ConnectedNodes = new List<string> { "Node_Life_1", "Node_DR_3", "Node_Life_3" },
    StatType = StatTypes.LifeRegen,
    Value = 2f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/Mend",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 2f
};

// Tier 4
AllNodes["Node_DR_3"] = new PassiveNode {
    ID = "Node_DR_3",
    Name = "Bulwark",
    Description = "Major: +15% Damage Reduction, -10% Movement Speed",
    Position = Layout.Grid(0f, 4f),
    ConnectedNodes = new List<string> { "Node_Regen_2" },
    StatType = StatTypes.DamageReduction,
    Value = 0.15f,
    IsMajor = true,
    IconPath = "ExileTree/Content/Assets/Passives/Bulwark",
    SkillPointCost = 2,
    MaxRank = 1,
    ValuePerRank = 0f
};

// Major Node (Tier 5 - extra gap)
AllNodes["Node_Life_3"] = new PassiveNode {
    ID = "Node_Life_3",
    Name = "Bound Vigor",
    Description = "Major: +60 Maximum Life, +1 Life Regeneration per second",
    Position = Layout.Grid(0f, 5.17f),
    ConnectedNodes = new List<string> { "Node_Regen_2" },
    StatType = StatTypes.Life,
    Value = 60f,
    IsMajor = true,
    IconPath = "ExileTree/Content/Assets/Passives/BoundVigor",
    SkillPointCost = 2,
    MaxRank = 1,
    ValuePerRank = 0f
};

// --------------------------------------------------
// 🎯 RANGED TREE (Center-Right - Column 4)
// --------------------------------------------------

// Tier 1
AllNodes["Node_Ranged_1"] = new PassiveNode {
    ID = "Node_Ranged_1",
    Name = "Sure Shot",
    Description = "+5% Ranged Damage per rank (Max 3)",
    Position = Layout.Grid(1f, 1f),
    ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Ranged_2" },
    StatType = StatTypes.RangedDamage,
    Value = 0.05f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/SureShot",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 0.05f
};

// Tier 2
AllNodes["Node_Ranged_2"] = new PassiveNode {
    ID = "Node_Ranged_2",
    Name = "Eagle Eye",
    Description = "+5% Ranged Crit Chance per rank (Max 3)",
    Position = Layout.Grid(1f, 2f),
    ConnectedNodes = new List<string> { "Node_Ranged_1", "Node_Move_2" },
    StatType = StatTypes.RangedCrit,
    Value = 5f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/EagleEye",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 5f
};

// Tier 3
AllNodes["Node_Move_2"] = new PassiveNode {
    ID = "Node_Move_2",
    Name = "Quick Step",
    Description = "+5% Movement Speed per rank (Max 3)",
    Position = Layout.Grid(1f, 3f),
    ConnectedNodes = new List<string> { "Node_Ranged_2", "Node_Ranged_3", "Node_Move_3" },
    StatType = StatTypes.MoveSpeed,
    Value = 0.05f,
    IsMajor = false,
    IconPath = "ExileTree/Content/Assets/Passives/QuickStep",
    SkillPointCost = 1,
    MaxRank = 3,
    ValuePerRank = 0.05f
};

// Major Node (Tier 4)
AllNodes["Node_Ranged_3"] = new PassiveNode {
    ID = "Node_Ranged_3",
    Name = "Marksman",
    Description = "Major: +15% Ranged Damage, +10% Ranged Crit, 20% Ammo Conservation",
    Position = Layout.Grid(1f, 4f),
    ConnectedNodes = new List<string> { "Node_Move_2" },
    StatType = StatTypes.RangedDamage,
    Value = 0.15f,
    IsMajor = true,
    IconPath = "ExileTree/Content/Assets/Passives/Marksman",
    SkillPointCost = 2,
    MaxRank = 1,
    ValuePerRank = 0f
};

// Major Node (Tier 5 - extra gap)
AllNodes["Node_Move_3"] = new PassiveNode {
    ID = "Node_Move_3",
    Name = "Lightning Step",
    Description = "Major: +20% Movement Speed, +Jump Speed",
    Position = Layout.Grid(1f, 5.17f),
    ConnectedNodes = new List<string> { "Node_Move_2" },
    StatType = StatTypes.MoveSpeed,
    Value = 0.20f,
    IsMajor = true,
    IconPath = "ExileTree/Content/Assets/Passives/LightningStep",
    SkillPointCost = 2,
    MaxRank = 1,
    ValuePerRank = 0f
};

            StarterNodeId = "Node_MaxHP";
            
            // Debug: test if your PNG is being found
            Main.NewText($"HasAsset({AllNodes["Node_MaxHP"].IconPath}) = {ModContent.HasAsset(AllNodes["Node_MaxHP"].IconPath)}", Color.Yellow);
        }

        // ------------------------------------------------------------------
        // Unlock + Effect Logic
        // ------------------------------------------------------------------
        // Helper method to determine which tree a node belongs to
        private static string GetTreeForNode(string nodeId)
        {
            if (nodeId.Contains("Melee")) return "Melee";
            if (nodeId.Contains("Magic")) return "Magic";
            if (nodeId.Contains("Ranged") || nodeId.Contains("Move")) return "Ranged";
            if (nodeId.Contains("Summon")) return "Summon";
            if (nodeId.Contains("DR") || nodeId.Contains("Life") || nodeId.Contains("Regen")) return "Health";
            return "";
        }

        // Helper method to check if two nodes belong to the same tree
        private static bool IsSameTree(string tree1, string tree2)
        {
            return !string.IsNullOrEmpty(tree1) && tree1 == tree2;
        }

        public static bool CanUnlock(string nodeId, HashSet<string> unlocked, Dictionary<string, int> nodeRanks = null)
        {
            if (!AllNodes.ContainsKey(nodeId)) return false;
            if (unlocked.Count == 0) return nodeId == StarterNodeId;

            var targetNode = AllNodes[nodeId];
            
            // If it's a major node (capstone), check tree investment instead of prerequisites
            if (targetNode.IsMajor && nodeRanks != null)
            {
                string targetTree = GetTreeForNode(nodeId);
                
                // Count points spent in this tree
                int pointsSpentInTree = 0;
                foreach (var kvp in nodeRanks)
                {
                    if (AllNodes.ContainsKey(kvp.Key))
                    {
                        var node = AllNodes[kvp.Key];
                        
                        // Only count non-major nodes from the same tree
                        if (!node.IsMajor && IsSameTree(targetTree, GetTreeForNode(kvp.Key)))
                        {
                            pointsSpentInTree += kvp.Value * node.SkillPointCost;
                        }
                    }
                }
                
                // Need at least 6 points spent in the tree
                if (pointsSpentInTree < 6) return false;
                
                // Major nodes don't need direct prerequisites - only tree investment
                return true;
            }
            
            // For non-major nodes, check if any connected node is unlocked (basic prerequisite)
            foreach (var other in unlocked)
            {
                if (AllNodes.ContainsKey(other) && AllNodes[other].ConnectedNodes.Contains(nodeId))
                {
                    return true;
                }
            }
            
            return false;
        }

        public static void ApplyNodeEffect(ExileTreePlayer player, string nodeId)
        {
            if (!AllNodes.ContainsKey(nodeId))
                return;

            var p = player.Player;

            switch (nodeId)
            {
                // --- Melee ---
                case "Node_Melee_1": p.GetDamage(DamageClass.Melee) += 0.05f; break;
                case "Node_Melee_2": p.GetAttackSpeed(DamageClass.Melee) += 0.05f; break;
                case "Node_Melee_3": p.GetCritChance(DamageClass.Melee) += 5; break;
                case "Node_MeleeMajor_BladedDance": p.GetAttackSpeed(DamageClass.Melee) += 0.15f; p.moveSpeed += 0.10f; break;
                case "Node_MeleeMajor_IronVanguard": p.GetDamage(DamageClass.Melee) += 0.10f; p.statDefense += 25; p.moveSpeed -= 0.10f; break;

                // --- Summoner ---
                case "Node_Summon_1": p.GetDamage(DamageClass.Summon) += 0.05f; break;
                case "Node_Summon_2": p.GetCritChance(DamageClass.Summon) += 5; break;
                case "Node_Summon_3": p.maxTurrets += 1; break;
                case "Node_SummonMajor_SpiritAscendant": 
                    // +25% summon damage when at max minions
                    if (p.numMinions >= p.maxMinions && p.maxMinions > 0)
                        p.GetDamage(DamageClass.Summon) += 0.25f;
                    break;
                case "Node_SummonMajor_DominionOfSouls": p.GetDamage(DamageClass.Summon) += 0.15f; p.maxMinions += 3; break;

                // --- Life ---
                case "Node_MaxHP": p.statLifeMax2 += 10; p.statDefense += 2; p.moveSpeed += 0.05f; break;
                case "Node_Life_1": p.statLifeMax2 += 20; break;
                case "Node_Life_3": p.statLifeMax2 += 60; p.lifeRegen += 2; break;

                // --- Regen ---
                case "Node_Regen_2": p.lifeRegen += 2; break;

                // --- Defense / DR ---
                case "Node_DR_1": p.endurance += 0.05f; break;
                case "Node_DR_3": p.endurance += 0.15f; p.moveSpeed -= 0.10f; break;

                // --- Movement ---
                case "Node_Move_2": p.moveSpeed += 0.05f; break;
                case "Node_Move_3": p.moveSpeed += 0.20f; p.jumpSpeedBoost += 0.8f; break;

                // --- Magic ---
                case "Node_Magic_1": p.GetDamage(DamageClass.Magic) += 0.05f; break;
                case "Node_Magic_2": p.GetCritChance(DamageClass.Magic) += 5; break;
                case "Node_Magic_4": p.statManaMax2 += 10; break;
                case "Node_Magic_3": p.GetDamage(DamageClass.Magic) += 0.15f; p.GetCritChance(DamageClass.Magic) += 10; p.statManaMax2 += 40; break;
                case "Node_MagicMajor_Spellweaver": p.manaCost -= 0.25f; p.manaRegenBonus += 10; break;

                // --- Ranged ---
                case "Node_Ranged_1": p.GetDamage(DamageClass.Ranged) += 0.05f; break;
                case "Node_Ranged_2": p.GetCritChance(DamageClass.Ranged) += 5; break;
                case "Node_Ranged_3": p.GetDamage(DamageClass.Ranged) += 0.15f; p.GetCritChance(DamageClass.Ranged) += 10; p.ammoCost80 = true; break;
            }
        }
    }
}

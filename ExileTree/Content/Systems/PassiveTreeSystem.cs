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
        public const float SPACING_X = 50f;
        public const float SPACING_Y = 40f;
        public static readonly Vector2 ORIGIN = new Vector2(200, 200);

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
AllNodes["Node_MaxHP"] = new PassiveNode {
    ID = "Node_MaxHP",
    Name = "Vital Spark",
    Description = "+20 Maximum Life",
    Position = Layout.Grid(0f, 0f),
    ConnectedNodes = new List<string> {
        "Node_Melee_1", "Node_Summon_1", "Node_Life_1",
        "Node_Regen_1", "Node_Move_1", "Node_DR_1",
        "Node_Magic_1", "Node_Ranged_1"
    },
    StatType = StatTypes.Life,
    Value = 20f,
    IsMajor = false
};

// --------------------------------------------------
// ⚔️ MELEE BRANCH (LEFT) — forked
// --------------------------------------------------

// Shared melee path from center
AllNodes["Node_Melee_1"] = new PassiveNode {
    ID = "Node_Melee_1",
    Name = "Honed Edge",
    Description = "+6% Melee Damage",
    Position = Layout.Grid(-1f, 0f),
    ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Melee_2", "Node_Melee_3" },
    StatType = StatTypes.MeleeDamage,
    Value = 0.06f,
    IsMajor = false
};

// Upper path → Bladed Dance
AllNodes["Node_Melee_2"] = new PassiveNode {
    ID = "Node_Melee_2",
    Name = "Quick Strikes",
    Description = "+7% Melee Attack Speed",
    Position = Layout.Grid(-2f, -0.5f),
    ConnectedNodes = new List<string> { "Node_Melee_1", "Node_MeleeMajor_BladedDance" },
    StatType = StatTypes.MeleeSpeed,
    Value = 0.07f,
    IsMajor = false
};

AllNodes["Node_MeleeMajor_BladedDance"] = new PassiveNode {
    ID = "Node_MeleeMajor_BladedDance",
    Name = "Bladed Dance",
    Description = "Major: +12% Melee Attack Speed, +8% Movement Speed",
    Position = Layout.Grid(-3f, -1f),
    ConnectedNodes = new List<string> { "Node_Melee_2" },
    StatType = StatTypes.MeleeSpeed,
    Value = 0.12f,
    IsMajor = true
};

// Lower path → Iron Vanguard
AllNodes["Node_Melee_3"] = new PassiveNode {
    ID = "Node_Melee_3",
    Name = "Bulwark",
    Description = "+6 Defense",
    Position = Layout.Grid(-2f, 0.5f),
    ConnectedNodes = new List<string> { "Node_Melee_1", "Node_MeleeMajor_IronVanguard" },
    StatType = StatTypes.Defense,
    Value = 6f,
    IsMajor = false
};

AllNodes["Node_MeleeMajor_IronVanguard"] = new PassiveNode {
    ID = "Node_MeleeMajor_IronVanguard",
    Name = "Iron Vanguard",
    Description = "Major: +10% Melee Damage, +25 Defense, -10% Movement Speed",
    Position = Layout.Grid(-3f, 1f),
    ConnectedNodes = new List<string> { "Node_Melee_3" },
    StatType = StatTypes.MeleeDamage,
    Value = 0.10f,
    IsMajor = true
};

// --------------------------------------------------
// 🕷️ SUMMONER BRANCH (RIGHT) — forked
// --------------------------------------------------

// Shared summoner path from center
AllNodes["Node_Summon_1"] = new PassiveNode {
    ID = "Node_Summon_1",
    Name = "Spirit Bond",
    Description = "+8% Summon Damage",
    Position = Layout.Grid(1f, 0f),
    ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Summon_2", "Node_Summon_3" },
    StatType = StatTypes.SummonDamage,
    Value = 0.08f,
    IsMajor = false
};

// Upper path → Primal Wrath
AllNodes["Node_Summon_2"] = new PassiveNode {
    ID = "Node_Summon_2",
    Name = "Soul Whisper",
    Description = "+6% Summon Damage",
    Position = Layout.Grid(2f, -0.5f),
    ConnectedNodes = new List<string> { "Node_Summon_1", "Node_SummonMajor_PrimalWrath" },
    StatType = StatTypes.SummonDamage,
    Value = 0.06f,
    IsMajor = false
};

AllNodes["Node_SummonMajor_PrimalWrath"] = new PassiveNode {
    ID = "Node_SummonMajor_PrimalWrath",
    Name = "Primal Wrath",
    Description = "Major: +25% Summon Damage while at Max Minions",
    Position = Layout.Grid(3f, -1f),
    ConnectedNodes = new List<string> { "Node_Summon_2" },
    StatType = StatTypes.SummonDamage,
    Value = 0f,
    IsMajor = true
};

// Lower path → Hive Mind
AllNodes["Node_Summon_3"] = new PassiveNode {
    ID = "Node_Summon_3",
    Name = "Legion Commander",
    Description = "+1 Minion Slot",
    Position = Layout.Grid(2f, 0.5f),
    ConnectedNodes = new List<string> { "Node_Summon_1", "Node_SummonMajor_HiveMind" },
    StatType = StatTypes.SummonSlot,
    Value = 1f,
    IsMajor = false
};

AllNodes["Node_SummonMajor_HiveMind"] = new PassiveNode {
    ID = "Node_SummonMajor_HiveMind",
    Name = "Hive Mind",
    Description = "Major: +12% Summon Damage, +2 Minion Slots",
    Position = Layout.Grid(3f, 1f),
    ConnectedNodes = new List<string> { "Node_Summon_3" },
    StatType = StatTypes.SummonDamage,
    Value = 0.12f,
    IsMajor = true
};

            // --------------------------------------------------
            // ❤️ LIFE BRANCH (UP)
            // --------------------------------------------------
            AllNodes["Node_Life_1"] = new PassiveNode {
                ID = "Node_Life_1",
                Name = "Vital Reserves",
                Description = "+20 Maximum Life",
                Position = Layout.Grid(0f, -1f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Life_2" },
                StatType = StatTypes.Life,
                Value = 20f,
                IsMajor = false
            };
            AllNodes["Node_Life_2"] = new PassiveNode {
                ID = "Node_Life_2",
                Name = "Sanguine Wellspring",
                Description = "+20 Maximum Life",
                Position = Layout.Grid(0f, -2f),
                ConnectedNodes = new List<string> { "Node_Life_1", "Node_Life_3" },
                StatType = StatTypes.Life,
                Value = 20f,
                IsMajor = false
            };
            AllNodes["Node_Life_3"] = new PassiveNode {
                ID = "Node_Life_3",
                Name = "Heart of Oak",
                Description = "Major: +60 Maximum Life, +1 Life Regeneration",
                Position = Layout.Grid(0f, -3f),
                ConnectedNodes = new List<string> { "Node_Life_2" },
                StatType = StatTypes.Life,
                Value = 60f,
                IsMajor = true
            };

            // --------------------------------------------------
            // 🔥 REGEN BRANCH (UP-LEFT)
            // --------------------------------------------------
            AllNodes["Node_Regen_1"] = new PassiveNode {
                ID = "Node_Regen_1",
                Name = "Second Wind",
                Description = "+1 Life Regeneration",
                Position = Layout.Grid(-1f, -1f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Regen_2" },
                StatType = StatTypes.LifeRegen,
                Value = 2f,
                IsMajor = false
            };
            AllNodes["Node_Regen_2"] = new PassiveNode {
                ID = "Node_Regen_2",
                Name = "Slow Mending",
                Description = "+1 Life Regeneration",
                Position = Layout.Grid(-2f, -2f),
                ConnectedNodes = new List<string> { "Node_Regen_1", "Node_Regen_3" },
                StatType = StatTypes.LifeRegen,
                Value = 2f,
                IsMajor = false
            };
            AllNodes["Node_Regen_3"] = new PassiveNode {
                ID = "Node_Regen_3",
                Name = "Phoenix Spirit",
                Description = "Major: +3 Life Regeneration, +10% Move Speed under 50% Life",
                Position = Layout.Grid(-3f, -3f),
                ConnectedNodes = new List<string> { "Node_Regen_2" },
                StatType = StatTypes.LifeRegen,
                Value = 6f,
                IsMajor = true
            };

            // --------------------------------------------------
            // 🧱 DEFENSE / DR BRANCH (DOWN-LEFT)
            // --------------------------------------------------
            AllNodes["Node_DR_1"] = new PassiveNode {
                ID = "Node_DR_1",
                Name = "Stone Skin",
                Description = "+4% Damage Reduction",
                Position = Layout.Grid(-1f, 1f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_DR_2" },
                StatType = StatTypes.DamageReduction,
                Value = 0.04f,
                IsMajor = false
            };
            AllNodes["Node_DR_2"] = new PassiveNode {
                ID = "Node_DR_2",
                Name = "Shield Training",
                Description = "+6 Defense",
                Position = Layout.Grid(-2f, 2f),
                ConnectedNodes = new List<string> { "Node_DR_1", "Node_DR_3" },
                StatType = StatTypes.Defense,
                Value = 6f,
                IsMajor = false
            };
            AllNodes["Node_DR_3"] = new PassiveNode {
                ID = "Node_DR_3",
                Name = "Stone Form",
                Description = "Major: +12% Damage Reduction, -8% Movement Speed",
                Position = Layout.Grid(-3f, 3f),
                ConnectedNodes = new List<string> { "Node_DR_2" },
                StatType = StatTypes.DamageReduction,
                Value = 0.12f,
                IsMajor = true
            };

            // --------------------------------------------------
            // 🏃 MOVE BRANCH (UP-RIGHT)
            // --------------------------------------------------
            AllNodes["Node_Move_1"] = new PassiveNode {
                ID = "Node_Move_1",
                Name = "Scout's Step",
                Description = "+6% Movement Speed",
                Position = Layout.Grid(1f, -1f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Move_2" },
                StatType = StatTypes.MoveSpeed,
                Value = 0.06f,
                IsMajor = false
            };
            AllNodes["Node_Move_2"] = new PassiveNode {
                ID = "Node_Move_2",
                Name = "Wind in the Veins",
                Description = "+6% Movement Speed",
                Position = Layout.Grid(2f, -2f),
                ConnectedNodes = new List<string> { "Node_Move_1", "Node_Move_3" },
                StatType = StatTypes.MoveSpeed,
                Value = 0.06f,
                IsMajor = false
            };
            AllNodes["Node_Move_3"] = new PassiveNode {
                ID = "Node_Move_3",
                Name = "Windrunner",
                Description = "Major: +20% Movement Speed, +Jump Speed",
                Position = Layout.Grid(3f, -3f),
                ConnectedNodes = new List<string> { "Node_Move_2" },
                StatType = StatTypes.MoveSpeed,
                Value = 0.20f,
                IsMajor = true
            };

            // --------------------------------------------------
            // ✨ MAGIC BRANCH (DOWN)
            // --------------------------------------------------
            AllNodes["Node_Magic_1"] = new PassiveNode {
                ID = "Node_Magic_1",
                Name = "Arcane Spark",
                Description = "+8% Magic Damage",
                Position = Layout.Grid(0f, 1f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Magic_2" },
                StatType = StatTypes.MagicDamage,
                Value = 0.08f,
                IsMajor = false
            };
            AllNodes["Node_Magic_2"] = new PassiveNode {
                ID = "Node_Magic_2",
                Name = "Arcane Focus",
                Description = "+3% Magic Critical Strike Chance",
                Position = Layout.Grid(0f, 2f),
                ConnectedNodes = new List<string> { "Node_Magic_1", "Node_Magic_3" },
                StatType = StatTypes.MagicCrit,
                Value = 3f,
                IsMajor = false
            };
            AllNodes["Node_Magic_3"] = new PassiveNode {
                ID = "Node_Magic_3",
                Name = "Archmage",
                Description = "Major: +15% Magic Damage, +10% Magic Crit, +40 Max Mana",
                Position = Layout.Grid(0f, 3f),
                ConnectedNodes = new List<string> { "Node_Magic_2" },
                StatType = StatTypes.MagicDamage,
                Value = 0.15f,
                IsMajor = true
            };

            // --------------------------------------------------
            // 🎯 RANGED BRANCH (DOWN-RIGHT)
            // --------------------------------------------------
            AllNodes["Node_Ranged_1"] = new PassiveNode {
                ID = "Node_Ranged_1",
                Name = "Sure Shot",
                Description = "+8% Ranged Damage",
                Position = Layout.Grid(1f, 1f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Ranged_2" },
                StatType = StatTypes.RangedDamage,
                Value = 0.08f,
                IsMajor = false
            };
            AllNodes["Node_Ranged_2"] = new PassiveNode {
                ID = "Node_Ranged_2",
                Name = "Eagle Eye",
                Description = "+4% Ranged Crit Chance",
                Position = Layout.Grid(2f, 2f),
                ConnectedNodes = new List<string> { "Node_Ranged_1", "Node_Ranged_3" },
                StatType = StatTypes.RangedCrit,
                Value = 4f,
                IsMajor = false
            };
            AllNodes["Node_Ranged_3"] = new PassiveNode {
                ID = "Node_Ranged_3",
                Name = "Marksman",
                Description = "Major: +15% Ranged Damage, +10% Ranged Crit, 20% Ammo Conservation",
                Position = Layout.Grid(3f, 3f),
                ConnectedNodes = new List<string> { "Node_Ranged_2" },
                StatType = StatTypes.RangedDamage,
                Value = 0.15f,
                IsMajor = true
            };

            StarterNodeId = "Node_MaxHP";
            
            // Debug: test if your PNG is being found
            Main.NewText($"HasAsset({AllNodes["Node_MaxHP"].IconPath}) = {ModContent.HasAsset(AllNodes["Node_MaxHP"].IconPath)}", Color.Yellow);
        }

        // ------------------------------------------------------------------
        // Unlock + Effect Logic
        // ------------------------------------------------------------------
        public static bool CanUnlock(string nodeId, HashSet<string> unlocked)
        {
            if (!AllNodes.ContainsKey(nodeId)) return false;
            if (unlocked.Count == 0) return nodeId == StarterNodeId;

            foreach (var other in unlocked)
                if (AllNodes.ContainsKey(other) && AllNodes[other].ConnectedNodes.Contains(nodeId))
                    return true;

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
                case "Node_Melee_1": p.GetDamage(DamageClass.Melee) += 0.06f; break;
                case "Node_Melee_2": p.GetAttackSpeed(DamageClass.Melee) += 0.07f; break;
                case "Node_Melee_3": p.statDefense += 6; break;
                case "Node_MeleeMajor_IronVanguard": p.GetDamage(DamageClass.Melee) += 0.10f; p.statDefense += 25; p.moveSpeed -= 0.10f; break;

                // --- Summoner ---
                case "Node_Summon_1": p.GetDamage(DamageClass.Summon) += 0.08f; break;
                case "Node_Summon_2": p.GetDamage(DamageClass.Summon) += 0.06f; break;
                case "Node_Summon_3": p.maxMinions += 1; break;
                case "Node_SummonMajor_HiveMind": p.GetDamage(DamageClass.Summon) += 0.12f; p.maxMinions += 2; break;

                // --- Life ---
                case "Node_Life_1": p.statLifeMax2 += 20; break;
                case "Node_Life_2": p.statLifeMax2 += 20; break;
                case "Node_Life_3": p.statLifeMax2 += 60; p.lifeRegen += 2; break;

                // --- Regen ---
                case "Node_Regen_1": p.lifeRegen += 2; break;
                case "Node_Regen_2": p.lifeRegen += 2; break;
                case "Node_Regen_3": p.lifeRegen += 6; if (p.statLife <= p.statLifeMax2 / 2) p.moveSpeed += 0.10f; break;

                // --- Defense / DR ---
                case "Node_DR_1": p.endurance += 0.04f; break;
                case "Node_DR_2": p.statDefense += 6; break;
                case "Node_DR_3": p.endurance += 0.12f; p.moveSpeed -= 0.08f; break;

                // --- Movement ---
                case "Node_Move_1": p.moveSpeed += 0.06f; break;
                case "Node_Move_2": p.moveSpeed += 0.06f; break;
                case "Node_Move_3": p.moveSpeed += 0.20f; p.jumpSpeedBoost += 0.8f; break;

                // --- Magic ---
                case "Node_Magic_1": p.GetDamage(DamageClass.Magic) += 0.08f; break;
                case "Node_Magic_2": p.GetCritChance(DamageClass.Magic) += 3; break;
                case "Node_Magic_3": p.GetDamage(DamageClass.Magic) += 0.15f; p.GetCritChance(DamageClass.Magic) += 10; p.statManaMax2 += 40; break;

                // --- Ranged ---
                case "Node_Ranged_1": p.GetDamage(DamageClass.Ranged) += 0.08f; break;
                case "Node_Ranged_2": p.GetCritChance(DamageClass.Ranged) += 4; break;
                case "Node_Ranged_3": p.GetDamage(DamageClass.Ranged) += 0.15f; p.GetCritChance(DamageClass.Ranged) += 10; p.ammoCost80 = true; break;
            }
        }
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader; // Player.GetDamage / GetCritChance / GetAttackSpeed

namespace ExileTree.Content.Systems
{
    // ---- Spacing & layout helpers (tweak here, affects entire tree) ----
    public static class Layout
    {
        // Distance between nodes on the X and Y axes (pixels per "grid unit")
        public const float SPACING_X = 80f;  // ← widen/tighten horizontally
        public const float SPACING_Y = 70f;   // ← widen/tighten vertically

        // Where the starter node sits inside the PassiveTree UI area (800x600)
        public static readonly Vector2 ORIGIN = new Vector2(200, 200);

        // Place a node at fractional grid coordinates relative to ORIGIN
        public static Vector2 Grid(float gx, float gy)
            => ORIGIN + new Vector2(gx * SPACING_X, gy * SPACING_Y);

        // Offset a concrete position by raw pixels (rarely needed)
        public static Vector2 Offset(Vector2 from, float dx, float dy)
            => from + new Vector2(dx, dy);
    }

    // Centralized stat keys used by nodes
    public static class StatTypes
    {
        public const string Life = "Life";
        public const string Defense = "Defense";
        public const string DamageReduction = "DamageReduction"; // player.endurance (+% DR)
        public const string LifeRegen = "LifeRegen";             // NOTE: +2 = +1 HP/sec

        public const string MeleeDamage = "MeleeDamage";
        public const string MeleeSpeed  = "MeleeSpeed";

        public const string SummonDamage = "SummonDamage";
        public const string SummonSlot   = "SummonSlot";   // +max minions
        public const string SentrySlot   = "SentrySlot";   // +max turrets (sentries)
        public const string SentryDamage = "SentryDamage"; // simple mode: maps to SummonDamage

        public const string MagicDamage  = "MagicDamage";
        public const string MagicCrit    = "MagicCrit";
        public const string ManaCost     = "ManaCost";     // negative reduces cost (e.g., -0.06f = -6%)
        public const string ManaBonus    = "ManaBonus";    // flat +max mana

        public const string RangedDamage = "RangedDamage";
        public const string RangedCrit   = "RangedCrit";

        public const string MoveSpeed = "MoveSpeed";
        public const string CritChance = "CritChance";     // (kept for future generic use)
    }

    public class PassiveTreeSystem
    {
        public class Node
        {
            public string ID;
            public string Name;
            public string Description;
            public Vector2 Position;              // Position inside our 800x600 UI area
            public List<string> ConnectedNodes;   // Adjacency for unlock rules
            public string StatType;               // See StatTypes
            public float Value;                   // e.g., 0.10f = +10% or 20f = +20 life
            public bool IsMajor;                  // For visuals (circle vs square, size, etc.)
        }

        public static Dictionary<string, Node> AllNodes = new();
        public static string StarterNodeId = "Node_MaxHP";

        /// <summary>
        /// Define the whole tree (Melee left, Summoner+Sentry right),
        /// plus Life, Defense/DR, LifeRegen, Movement (up/diagonals),
        /// and NEW: Magic (down center) and Ranged (down-right).
        /// Coordinates use Layout.Grid(gx, gy) so spacing is centralized.
        /// </summary>
        public static void LoadTree()
        {
            AllNodes.Clear();

            // --------------------------------------------------
            // STARTER (at Layout.ORIGIN / Grid(0,0))
            // --------------------------------------------------
            AllNodes["Node_MaxHP"] = new Node {
                ID = "Node_MaxHP",
                Name = "Vital Spark",
                Description = "+20 Maximum Life",
                Position = Layout.Grid(0f, 0f),
                ConnectedNodes = new List<string> {
                    "Node_MeleeCore", "Node_SummonCore",
                    "Node_Life_A", "Node_Regen_A", "Node_Move_A", "Node_DR_A",
                    "Node_MagicCore", "Node_RangedCore"
                },
                StatType = StatTypes.Life,
                Value = 20f,
                IsMajor = false
            };

            // --------------------------------------------------
            // ⚔️ MELEE BRANCH (LEFT): two distinct major endpoints
            // --------------------------------------------------

            AllNodes["Node_MeleeCore"] = new Node {
                ID = "Node_MeleeCore",
                Name = "Honed Edge",
                Description = "+6% Melee Damage",
                Position = Layout.Grid(-0.75f, 0f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_MeleeSpeed", "Node_MeleeDR" },
                StatType = StatTypes.MeleeDamage,
                Value = 0.06f,
                IsMajor = false
            };

            AllNodes["Node_MeleeSpeed"] = new Node {
                ID = "Node_MeleeSpeed",
                Name = "Quick Strikes",
                Description = "+7% Melee Attack Speed",
                Position = Layout.Grid(-1.5f, -0.75f),
                ConnectedNodes = new List<string> { "Node_MeleeCore", "Node_MeleeMajor_BladedDance" },
                StatType = StatTypes.MeleeSpeed,
                Value = 0.07f,
                IsMajor = false
            };

            AllNodes["Node_MeleeMajor_BladedDance"] = new Node {
                ID = "Node_MeleeMajor_BladedDance",
                Name = "Bladed Dance",
                Description = "Major: +12% Melee Attack Speed, +8% Movement Speed",
                Position = Layout.Grid(-2.25f, -1.5f),
                ConnectedNodes = new List<string> { "Node_MeleeSpeed" },
                StatType = StatTypes.MeleeSpeed, // base; extra handled in ApplyNodeEffect
                Value = 0.12f,
                IsMajor = true
            };

            AllNodes["Node_MeleeDR"] = new Node {
                ID = "Node_MeleeDR",
                Name = "Bulwark",
                Description = "+6 Defense",
                Position = Layout.Grid(-1.5f, 0.75f),
                ConnectedNodes = new List<string> { "Node_MeleeCore", "Node_MeleeMajor_IronVanguard" },
                StatType = StatTypes.Defense,
                Value = 6f,
                IsMajor = false
            };

            AllNodes["Node_MeleeMajor_IronVanguard"] = new Node {
                ID = "Node_MeleeMajor_IronVanguard",
                Name = "Iron Vanguard",
                Description = "Major: +10% Melee Damage, +25 Defense, -10% Movement Speed",
                Position = Layout.Grid(-2.3f, 1.4f),
                ConnectedNodes = new List<string> { "Node_MeleeDR" },
                StatType = StatTypes.MeleeDamage, // base; extras handled below
                Value = 0.10f,
                IsMajor = true
            };

            // --------------------------------------------------
            // 🕷️ SUMMONER BRANCH (RIGHT): split into Minions vs Sentries
            // --------------------------------------------------

            AllNodes["Node_SummonCore"] = new Node {
                ID = "Node_SummonCore",
                Name = "Spirit Bond",
                Description = "+8% Summon Damage",
                Position = Layout.Grid(0.75f, 0f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_SummonSpeed", "Node_SentryDmg" },
                StatType = StatTypes.SummonDamage,
                Value = 0.08f,
                IsMajor = false
            };

            // Upper minion sub-branch
            AllNodes["Node_SummonSpeed"] = new Node {
                ID = "Node_SummonSpeed",
                Name = "Soul Whisper",
                Description = "+6% Summon Damage",
                Position = Layout.Grid(1.5f, -0.75f),
                ConnectedNodes = new List<string> { "Node_SummonCore", "Node_SummonSlots" },
                StatType = StatTypes.SummonDamage,
                Value = 0.06f,
                IsMajor = false
            };

            AllNodes["Node_SummonSlots"] = new Node {
                ID = "Node_SummonSlots",
                Name = "Legion Commander",
                Description = "+1 Minion Slot",
                Position = Layout.Grid(2.25f, -1.25f),
                ConnectedNodes = new List<string> { "Node_SummonSpeed", "Node_SummonMajor_PrimalWrath" },
                StatType = StatTypes.SummonSlot,
                Value = 1f,
                IsMajor = false
            };

            AllNodes["Node_SummonMajor_PrimalWrath"] = new Node {
                ID = "Node_SummonMajor_PrimalWrath",
                Name = "Primal Wrath",
                Description = "Major: +25% Summon Damage while at Max Minions",
                Position = Layout.Grid(3.0f, -1.75f),
                ConnectedNodes = new List<string> { "Node_SummonSlots" },
                StatType = StatTypes.SummonDamage, // conditional handled below
                Value = 0f,
                IsMajor = true
            };

            // Lower sentry sub-branch
            AllNodes["Node_SentryDmg"] = new Node {
                ID = "Node_SentryDmg",
                Name = "Siegecraft",
                Description = "+10% Sentry Damage",
                Position = Layout.Grid(1.5f, 0.75f),
                ConnectedNodes = new List<string> { "Node_SummonCore", "Node_SentrySlot" },
                StatType = StatTypes.SentryDamage, // simple mode: maps to SummonDamage
                Value = 0.10f,
                IsMajor = false
            };

            AllNodes["Node_SentrySlot"] = new Node {
                ID = "Node_SentrySlot",
                Name = "Field Engineer",
                Description = "+1 Sentry Slot",
                Position = Layout.Grid(2.25f, 1.25f),
                ConnectedNodes = new List<string> { "Node_SentryDmg", "Node_SummonMajor_HiveMind" },
                StatType = StatTypes.SentrySlot,
                Value = 1f,
                IsMajor = false
            };

            AllNodes["Node_SummonMajor_HiveMind"] = new Node {
                ID = "Node_SummonMajor_HiveMind",
                Name = "Hive Mind",
                Description = "Major: +12% Summon Damage, +2 Minion Slots",
                Position = Layout.Grid(3.0f, 1.75f),
                ConnectedNodes = new List<string> { "Node_SentrySlot" },
                StatType = StatTypes.SummonDamage, // base; extra slots handled below
                Value = 0.12f,
                IsMajor = true
            };

            // --------------------------------------------------
            // ❤️ LIFE BRANCH (UP)
            // --------------------------------------------------

            AllNodes["Node_Life_A"] = new Node {
                ID = "Node_Life_A",
                Name = "Vital Reserves",
                Description = "+20 Maximum Life",
                Position = Layout.Grid(0f, -0.75f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Life_B" },
                StatType = StatTypes.Life,
                Value = 20f,
                IsMajor = false
            };

            AllNodes["Node_Life_B"] = new Node {
                ID = "Node_Life_B",
                Name = "Sanguine Wellspring",
                Description = "+20 Maximum Life",
                Position = Layout.Grid(0f, -1.5f),
                ConnectedNodes = new List<string> { "Node_Life_A", "Node_LifeMajor_HeartOfOak" },
                StatType = StatTypes.Life,
                Value = 20f,
                IsMajor = false
            };

            AllNodes["Node_LifeMajor_HeartOfOak"] = new Node {
                ID = "Node_LifeMajor_HeartOfOak",
                Name = "Heart of Oak",
                Description = "Major: +60 Maximum Life, +1 Life Regeneration",
                Position = Layout.Grid(0f, -2.25f),
                ConnectedNodes = new List<string> { "Node_Life_B" },
                StatType = StatTypes.Life, // base 60 life; extra regen handled below
                Value = 60f,
                IsMajor = true
            };

            // --------------------------------------------------
            // 🔥 LIFE REGEN BRANCH (UP-LEFT)
            // --------------------------------------------------

            AllNodes["Node_Regen_A"] = new Node {
                ID = "Node_Regen_A",
                Name = "Second Wind",
                Description = "+1 Life Regeneration",
                Position = Layout.Grid(-0.75f, -1.0f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Regen_B" },
                StatType = StatTypes.LifeRegen,
                Value = 2f, // +1 HP/sec
                IsMajor = false
            };

            AllNodes["Node_Regen_B"] = new Node {
                ID = "Node_Regen_B",
                Name = "Slow Mending",
                Description = "+1 Life Regeneration",
                Position = Layout.Grid(-1.5f, -1.75f),
                ConnectedNodes = new List<string> { "Node_Regen_A", "Node_RegenMajor_Phoenix" },
                StatType = StatTypes.LifeRegen,
                Value = 2f, // +1 HP/sec
                IsMajor = false
            };

            AllNodes["Node_RegenMajor_Phoenix"] = new Node {
                ID = "Node_RegenMajor_Phoenix",
                Name = "Phoenix Spirit",
                Description = "Major: +3 Life Regeneration, +10% Move Speed while under 50% Life",
                Position = Layout.Grid(-2.25f, -2.5f),
                ConnectedNodes = new List<string> { "Node_Regen_B" },
                StatType = StatTypes.LifeRegen, // base +3 regen; conditional mobility handled below
                Value = 6f, // +3 HP/sec
                IsMajor = true
            };

            // --------------------------------------------------
            // 🧱 DEFENSE / DR BRANCH (DOWN-LEFT)
            // --------------------------------------------------

            AllNodes["Node_DR_A"] = new Node {
                ID = "Node_DR_A",
                Name = "Stone Skin",
                Description = "+4% Damage Reduction",
                Position = Layout.Grid(-0.75f, 1.0f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_DEF_A" },
                StatType = StatTypes.DamageReduction,
                Value = 0.04f,
                IsMajor = false
            };

            AllNodes["Node_DEF_A"] = new Node {
                ID = "Node_DEF_A",
                Name = "Shield Training",
                Description = "+6 Defense",
                Position = Layout.Grid(-1.5f, 1.5f),
                ConnectedNodes = new List<string> { "Node_DR_A", "Node_DRMajor_StoneForm" },
                StatType = StatTypes.Defense,
                Value = 6f,
                IsMajor = false
            };

            AllNodes["Node_DRMajor_StoneForm"] = new Node {
                ID = "Node_DRMajor_StoneForm",
                Name = "Stone Form",
                Description = "Major: +12% Damage Reduction, -8% Movement Speed",
                Position = Layout.Grid(-2.25f, 2.0f),
                ConnectedNodes = new List<string> { "Node_DEF_A" },
                StatType = StatTypes.DamageReduction,
                Value = 0.12f,
                IsMajor = true
            };

            // --------------------------------------------------
            // 🏃 MOVEMENT BRANCH (UP-RIGHT)
            // --------------------------------------------------

            AllNodes["Node_Move_A"] = new Node {
                ID = "Node_Move_A",
                Name = "Scout's Step",
                Description = "+6% Movement Speed",
                Position = Layout.Grid(0.75f, -1.0f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_Move_B" },
                StatType = StatTypes.MoveSpeed,
                Value = 0.06f,
                IsMajor = false
            };

            AllNodes["Node_Move_B"] = new Node {
                ID = "Node_Move_B",
                Name = "Wind in the Veins",
                Description = "+6% Movement Speed",
                Position = Layout.Grid(1.5f, -1.75f),
                ConnectedNodes = new List<string> { "Node_Move_A", "Node_MoveMajor_Windrunner" },
                StatType = StatTypes.MoveSpeed,
                Value = 0.06f,
                IsMajor = false
            };

            AllNodes["Node_MoveMajor_Windrunner"] = new Node {
                ID = "Node_MoveMajor_Windrunner",
                Name = "Windrunner",
                Description = "Major: +20% Movement Speed, +Jump Speed",
                Position = Layout.Grid(2.25f, -2.5f),
                ConnectedNodes = new List<string> { "Node_Move_B" },
                StatType = StatTypes.MoveSpeed, // base +20% move; extra jump handled below
                Value = 0.20f,
                IsMajor = true
            };

            // --------------------------------------------------
            // ✨ MAGIC BRANCH (DOWN CENTER)
            // Path: Starter -> MagicCore -> MagicCrit -> MagicMana -> MagicMajor_Archmage
            // --------------------------------------------------

            AllNodes["Node_MagicCore"] = new Node {
                ID = "Node_MagicCore",
                Name = "Arcane Spark",
                Description = "+8% Magic Damage",
                Position = Layout.Grid(0f, 0.75f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_MagicCrit" },
                StatType = StatTypes.MagicDamage,
                Value = 0.08f,
                IsMajor = false
            };

            AllNodes["Node_MagicCrit"] = new Node {
                ID = "Node_MagicCrit",
                Name = "Arcane Focus",
                Description = "+3% Magic Critical Strike Chance",
                Position = Layout.Grid(0f, 1.5f),
                ConnectedNodes = new List<string> { "Node_MagicCore", "Node_MagicMana" },
                StatType = StatTypes.MagicCrit,
                Value = 3f,
                IsMajor = false
            };

            AllNodes["Node_MagicMana"] = new Node {
                ID = "Node_MagicMana",
                Name = "Channeling",
                Description = "-6% Mana Cost",
                Position = Layout.Grid(0f, 2.25f),
                ConnectedNodes = new List<string> { "Node_MagicCrit", "Node_MagicMajor_Archmage" },
                StatType = StatTypes.ManaCost, // negative reduces mana cost
                Value = -0.06f,
                IsMajor = false
            };

            AllNodes["Node_MagicMajor_Archmage"] = new Node {
                ID = "Node_MagicMajor_Archmage",
                Name = "Archmage",
                Description = "Major: +15% Magic Damage, +10% Magic Crit, +40 Max Mana",
                Position = Layout.Grid(0f, 3.0f),
                ConnectedNodes = new List<string> { "Node_MagicMana" },
                StatType = StatTypes.MagicDamage, // base; extra handled below
                Value = 0.15f,
                IsMajor = true
            };

            // --------------------------------------------------
            // 🎯 RANGED BRANCH (DOWN-RIGHT)
            // Path: Starter -> RangedCore -> RangedCrit -> RangedDmg2 -> RangedMajor_Marksman
            // --------------------------------------------------

            AllNodes["Node_RangedCore"] = new Node {
                ID = "Node_RangedCore",
                Name = "Sure Shot",
                Description = "+8% Ranged Damage",
                Position = Layout.Grid(0.75f, 1.5f),
                ConnectedNodes = new List<string> { "Node_MaxHP", "Node_RangedCrit" },
                StatType = StatTypes.RangedDamage,
                Value = 0.08f,
                IsMajor = false
            };

            AllNodes["Node_RangedCrit"] = new Node {
                ID = "Node_RangedCrit",
                Name = "Eagle Eye",
                Description = "+4% Ranged Critical Strike Chance",
                Position = Layout.Grid(0.75f, 2.25f),
                ConnectedNodes = new List<string> { "Node_RangedCore", "Node_RangedDmg2" },
                StatType = StatTypes.RangedCrit,
                Value = 4f,
                IsMajor = false
            };

            AllNodes["Node_RangedDmg2"] = new Node {
                ID = "Node_RangedDmg2",
                Name = "Tactical Reload",
                Description = "+6% Ranged Damage",
                Position = Layout.Grid(0.75f, 3.0f),
                ConnectedNodes = new List<string> { "Node_RangedCrit", "Node_RangedMajor_Marksman" },
                StatType = StatTypes.RangedDamage,
                Value = 0.06f,
                IsMajor = false
            };

            AllNodes["Node_RangedMajor_Marksman"] = new Node {
                ID = "Node_RangedMajor_Marksman",
                Name = "Marksman",
                Description = "Major: +15% Ranged Damage, +10% Ranged Crit, 20% Ammo Conservation",
                Position = Layout.Grid(0.75f, 3.75f),
                ConnectedNodes = new List<string> { "Node_RangedDmg2" },
                StatType = StatTypes.RangedDamage, // base; extra handled below
                Value = 0.15f,
                IsMajor = true
            };

            StarterNodeId = "Node_MaxHP";
        }

        /// <summary>
        /// A node is unlockable if it's the starter (when nothing unlocked),
        /// or if it is adjacent to any already-unlocked node.
        /// </summary>
        public static bool CanUnlock(string nodeId, HashSet<string> unlocked)
        {
            if (!AllNodes.ContainsKey(nodeId)) return false;
            if (unlocked.Count == 0) return nodeId == StarterNodeId;

            foreach (var other in unlocked) {
                if (AllNodes.ContainsKey(other) && AllNodes[other].ConnectedNodes.Contains(nodeId))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Apply a single node's effect to the player. Called each tick by ExileTreePlayer.ResetEffects().
        /// Also handles special keystone behaviors by node ID.
        /// </summary>
        public static void ApplyNodeEffect(Player player, Node node)
        {
            switch (node.StatType)
            {
                case StatTypes.Life:
                    player.statLifeMax2 += (int)node.Value;
                    break;

                case StatTypes.Defense:
                    player.statDefense += (int)node.Value;
                    break;

                case StatTypes.DamageReduction:
                    player.endurance += node.Value; // +0.12f = +12% DR
                    break;

                case StatTypes.LifeRegen:
                    player.lifeRegen += (int)node.Value; // +2 = +1 HP/sec
                    break;

                case StatTypes.MeleeDamage:
                    player.GetDamage(DamageClass.Melee) += node.Value;
                    break;

                case StatTypes.MeleeSpeed:
                    player.GetAttackSpeed(DamageClass.Melee) += node.Value;
                    break;

                case StatTypes.MoveSpeed:
                    player.moveSpeed += node.Value;
                    break;

                case StatTypes.SummonDamage:
                    player.GetDamage(DamageClass.Summon) += node.Value;
                    break;

                case StatTypes.SummonSlot:
                    player.maxMinions += (int)node.Value;
                    break;

                case StatTypes.SentrySlot:
                    player.maxTurrets += (int)node.Value;
                    break;

                case StatTypes.SentryDamage:
                    // Simple mode: most sentries scale with Summon damage anyway.
                    player.GetDamage(DamageClass.Summon) += node.Value;
                    break;

                case StatTypes.MagicDamage:
                    player.GetDamage(DamageClass.Magic) += node.Value;
                    break;

                case StatTypes.MagicCrit:
                    player.GetCritChance(DamageClass.Magic) += (int)node.Value;
                    break;

                case StatTypes.ManaCost:
                    // Negative values reduce mana cost. (e.g., -0.06f = -6%)
                    player.manaCost += node.Value; // yes, adding a negative reduces cost
                    break;

                case StatTypes.ManaBonus:
                    player.statManaMax2 += (int)node.Value;
                    break;

                case StatTypes.RangedDamage:
                    player.GetDamage(DamageClass.Ranged) += node.Value;
                    break;

                case StatTypes.RangedCrit:
                    player.GetCritChance(DamageClass.Ranged) += (int)node.Value;
                    break;

                case StatTypes.CritChance:
                    player.GetCritChance(DamageClass.Melee)  += (int)node.Value;
                    player.GetCritChance(DamageClass.Ranged) += (int)node.Value;
                    player.GetCritChance(DamageClass.Magic)  += (int)node.Value;
                    break;

                default:
                    break;
            }

            // ----- Extra effects / conditions for majors -----

            // Bladed Dance: already applied 12% melee speed; add +8% move speed.
            if (node.ID == "Node_MeleeMajor_BladedDance") {
                player.moveSpeed += 0.08f;
            }

            // Iron Vanguard: already applied +10% melee damage; add +25 DEF and -10% move speed.
            if (node.ID == "Node_MeleeMajor_IronVanguard") {
                player.statDefense += 25;
                player.moveSpeed -= 0.10f;
            }

            // Primal Wrath: +25% summon damage while at max minions.
            if (node.ID == "Node_SummonMajor_PrimalWrath") {
                int owned = player.numMinions;
                if (owned >= player.maxMinions) {
                    player.GetDamage(DamageClass.Summon) += 0.25f;
                }
            }

            // Hive Mind: already applied +12% summon damage; also +2 minion slots.
            if (node.ID == "Node_SummonMajor_HiveMind") {
                player.maxMinions += 2;
            }

            // Heart of Oak: base +60 life; also +1 life regen.
            if (node.ID == "Node_LifeMajor_HeartOfOak") {
                player.lifeRegen += 2; // +1 HP/sec
            }

            // Phoenix Spirit: base +3 regen; if under 50% life, +10% move speed.
            if (node.ID == "Node_RegenMajor_Phoenix") {
                if (player.statLife <= player.statLifeMax2 / 2) {
                    player.moveSpeed += 0.10f;
                }
            }

            // Windrunner: base +20% move speed; also buff jump speed.
            if (node.ID == "Node_MoveMajor_Windrunner") {
                player.jumpSpeedBoost += 0.8f; // noticeable extra jump; tweak to taste
            }

            // Archmage: already applied +15% magic dmg; also +10% magic crit and +40 mana.
            if (node.ID == "Node_MagicMajor_Archmage") {
                player.GetCritChance(DamageClass.Magic) += 10;
                player.statManaMax2 += 40;
            }

            // Marksman: already applied +15% ranged dmg; also +10% ranged crit and ammo conservation (20%).
            if (node.ID == "Node_RangedMajor_Marksman") {
                player.GetCritChance(DamageClass.Ranged) += 10;
                player.ammoCost80 = true; // Terraria flag: 20% chance not to consume ammo
            }

            // Stone Form: already gave +12% DR; also -8% move speed.
            if (node.ID == "Node_DRMajor_StoneForm") {
                player.moveSpeed -= 0.08f;
            }
        }
    }
}

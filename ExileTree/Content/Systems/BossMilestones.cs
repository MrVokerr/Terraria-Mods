using Terraria;

namespace ExileTree.Content.Systems
{
    // Central place to define which bosses grant a passive point.
    // We use common progression "milestones" — feels right for most playthroughs.
    public static class BossMilestones
    {
        // Returns how many milestone bosses are defeated in the CURRENT world.
        public static int GetMilestoneCount()
        {
            int c = 0;

            // Pre-Hardmode
            if (NPC.downedSlimeKing) c++;           // King Slime
            if (NPC.downedBoss1) c++;               // Eye of Cthulhu
            if (NPC.downedBoss2) c++;               // Eater of Worlds OR Brain of Cthulhu
            if (NPC.downedQueenBee) c++;            // Queen Bee
            if (NPC.downedBoss3) c++;               // Skeletron
            if (NPC.downedDeerclops) c++;           // Deerclops (optional but common)
            if (Main.hardMode) c++;                 // Wall of Flesh (world flag -> Hardmode)

            // Early Hardmode
            if (NPC.downedQueenSlime) c++;          // Queen Slime
            if (NPC.downedMechBoss1) c++;           // The Twins
            if (NPC.downedMechBoss2) c++;           // The Destroyer
            if (NPC.downedMechBoss3) c++;           // Skeletron Prime

            // Mid/late Hardmode
            if (NPC.downedPlantBoss) c++;           // Plantera
            if (NPC.downedGolemBoss) c++;           // Golem
            if (NPC.downedEmpressOfLight) c++;      // Empress of Light (optional but common)
            if (NPC.downedFishron) c++;             // Duke Fishron
            if (NPC.downedAncientCultist) c++;      // Lunatic Cultist
            if (NPC.downedMoonlord) c++;            // Moon Lord

            return c;
        }
    }
}

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VokerrsBosses.Common.Players
{
	public class PlatformDisablePlayer : ModPlayer
	{
		public override void PreUpdateMovement()
		{
			// Check if Mr. Game & Watch boss is active
			bool bossActive = false;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && npc.type == ModContent.NPCType<Content.NPCs.Bosses.MrGameAndWatch.MrGameAndWatch>())
				{
					bossActive = true;
					break;
				}
			}

			if (bossActive)
			{
				// Make platforms completely non-solid by forcing controlDown to true
				// This simulates holding the DOWN key, which makes player phase through platforms
				Player.controlDown = true;
			}
		}
	}
}

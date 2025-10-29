using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VokerrsBosses.Content.Items.SummonItems
{
	public class FlatBeep : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Flat Beep");
			// Tooltip.SetDefault("A mysterious floppy disc from 1980\n'BEEP BEEP BEEP!'\nSummons the 2D retro fighter");
			
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 12; // Boss summon priority
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 20;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = true;
		}

		public override bool CanUseItem(Player player)
		{
			// Can only be used if Mr. Game and Watch is not already present
			return !NPC.AnyNPCs(ModContent.NPCType<NPCs.Bosses.MrGameAndWatch.MrGameAndWatch>());
		}

	public override bool? UseItem(Player player)
	{
		if (player.whoAmI == Main.myPlayer)
		{
			// Funny summoning message
			string[] summonMessages = new string[]
			{
				"The ancient Game & Watch beeps to life! BEEP BEEP BEEP!",
				"You pressed the wrong button sequence! Mr. Game & Watch appears!",
				"The LCD screen flickers... something 2D approaches!",
				"*BEEP* *BEEP* *BEEP* - Someone's high score has been challenged!",
				"A wild 2D fighter appears from the digital realm!",
				"ERROR 404: Dimension Not Found. Mr. Game & Watch has arrived!",
				"You've activated a retro gaming curse! Mr. Game & Watch materializes!"
			};
			
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				Main.NewText(Main.rand.Next(summonMessages), 200, 200, 200);
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				Terraria.Chat.ChatHelper.BroadcastChatMessage(
					Terraria.Localization.NetworkText.FromLiteral(Main.rand.Next(summonMessages)),
					new Color(200, 200, 200));
			}
			
			// Spawn the boss with dramatic effect
			SoundEngine.PlaySound(SoundID.Roar, player.position);
			
			// Spawn particles
			for (int i = 0; i < 30; i++)
			{
				Dust dust = Dust.NewDustDirect(player.position - new Vector2(50, 50), 100, 100, 
					DustID.Smoke, 0f, 0f, 100, Color.Gray, 2f);
				dust.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 6f);
				dust.noGravity = true;
			}
			
			int type = ModContent.NPCType<NPCs.Bosses.MrGameAndWatch.MrGameAndWatch>();
			
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				// Spawn close to the player - directly in front, on the ground
				Vector2 spawnPos = player.Center;
				
				// Spawn 200 pixels to the left or right of player (in front of where they're facing)
				spawnPos.X += player.direction * 200f;
				
				// Find ground level below spawn position (or use player's Y if no ground nearby)
				int tileX = (int)(spawnPos.X / 16f);
				int tileY = (int)(player.position.Y / 16f);
				
				// Search downward for solid ground
				bool foundGround = false;
				for (int y = tileY; y < tileY + 50; y++)
				{
					if (WorldGen.SolidTile(tileX, y))
					{
						spawnPos.Y = y * 16f - 32f; // Spawn just above the ground tile
						foundGround = true;
						break;
					}
				}
				
				if (!foundGround)
				{
					spawnPos.Y = player.position.Y; // Default to player's height if no ground found
				}
				
				NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), (int)spawnPos.X, (int)spawnPos.Y, type);
			}
			else
			{
				NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
			}
		}
		return true;
	}		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Ectoplasm, 5) // Post-Plantera Dungeon drop (same as Duke Fishron tier)
				.AddIngredient(ItemID.Glass, 10) // LCD screen
				.AddIngredient(ItemID.SoulofLight, 5) // Light souls for the LCD glow
				.AddIngredient(ItemID.SoulofNight, 5) // Night souls for the dark pixels
				.AddTile(TileID.MythrilAnvil) // Hardmode crafting station
				.Register();
		}
	}
}

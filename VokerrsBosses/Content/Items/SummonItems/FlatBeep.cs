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
			// Tooltip.SetDefault("Summons Mr. Game and Watch\n'Beep Beep Beep!'");
			
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
				// Spawn above and in front of the player
				NPC.SpawnOnPlayer(player.whoAmI, type);
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
				.AddIngredient(ItemID.Wood, 10)
				.AddIngredient(ItemID.Wire, 5)
				.AddIngredient(ItemID.Cog, 3)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}

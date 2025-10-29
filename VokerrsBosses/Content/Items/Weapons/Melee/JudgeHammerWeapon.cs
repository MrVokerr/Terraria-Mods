using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VokerrsBosses.Content.Projectiles.Weapons;

namespace VokerrsBosses.Content.Items.Weapons.Melee
{
	public class JudgeHammerWeapon : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Judge's Hammer");
			// Tooltip.SetDefault("Roll the dice of fate!\nRandomly deals 1-9 damage in an AoE\nRolling a 9 is legendary!\n'BEEP BEEP BEEP'");
		}

		public override void SetDefaults()
		{
			Item.damage = 100; // Base damage (will be multiplied by judge roll)
			Item.DamageType = DamageClass.Melee;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 300; // 5 seconds (60 ticks per second * 5)
			Item.useAnimation = 300;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6;
		Item.value = Item.buyPrice(gold: 10);
		Item.rare = ItemRarityID.Pink; // Hardmode tier
		Item.shootSpeed = 0f;
		Item.shoot = ModContent.ProjectileType<PlayerJudgeHammer>();
		Item.noMelee = true; // Projectile does the damage
			Item.noUseGraphic = true; // Hide item during use
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item1;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// Roll 1-9
			int judgeRoll = Main.rand.Next(1, 10);
			
			// Calculate damage: base * judgeRoll
			int finalDamage = damage * judgeRoll;
			
			// Rolling a 9 makes it instakill non-bosses (9999 damage)
			if (judgeRoll == 9)
			{
				finalDamage = 9999;
			}
			
			// Spawn the hammer projectile at player's position
			Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, finalDamage, knockback, player.whoAmI, judgeRoll);
			
			// Announce the roll
			string message = judgeRoll switch
			{
				9 => $"You rolled a {judgeRoll}! LEGENDARY INSTAKILL!",
				8 or 7 => $"You rolled a {judgeRoll}! Strong hit!",
				6 or 5 => $"You rolled a {judgeRoll}. Decent.",
				4 or 3 => $"You rolled a {judgeRoll}. Weak.",
				_ => $"You rolled a {judgeRoll}... pathetic."
			};
			
			Color messageColor = judgeRoll switch
			{
				9 => new Color(255, 215, 0),      // Gold
				8 or 7 => new Color(255, 165, 0), // Orange
				6 or 5 => Color.Yellow,
				4 or 3 => Color.White,
				_ => Color.Gray
			};
			
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				Main.NewText(message, messageColor.R, messageColor.G, messageColor.B);
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				Terraria.Chat.ChatHelper.BroadcastChatMessage(
					Terraria.Localization.NetworkText.FromLiteral(message), messageColor);
			}
			
			return false; // We manually spawned the projectile
		}

		public override void AddRecipes()
		{
			// No recipe - boss drop only
		}
	}
}

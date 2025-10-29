using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace VokerrsBosses.Content.Projectiles.Bosses.MrGameAndWatch
{
	public class JudgeHammer : ModProjectile
	{
		private int JudgeNumber => (int)Projectile.ai[0];
		private bool hasDealtDamage = false;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Judge Hammer");
		}

	public override void SetDefaults()
	{
		Projectile.width = 48;
		Projectile.height = 48;
		Projectile.hostile = true; // Boss projectile - damages players
		Projectile.friendly = false;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 45;
		Projectile.aiStyle = -1;
		Projectile.tileCollide = false;
	}

	public override void AI()
	{
		// Follow the NPC owner (Mr. Game & Watch) during windup
		if (Projectile.ai[1] < 20)
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && npc.type == ModContent.NPCType<NPCs.Bosses.MrGameAndWatch.MrGameAndWatch>())
				{
					Projectile.Center = npc.Center + new Vector2(npc.direction * 30, 0);
					Projectile.direction = npc.direction;
					Projectile.spriteDirection = npc.direction;
					break;
				}
			}
		}		// Swing animation
		Projectile.rotation = MathHelper.Lerp(-0.5f, 0.5f, Projectile.ai[1] / 30f) * Projectile.direction;
		Projectile.ai[1]++;

		// Spawn number above hammer
		if (Projectile.ai[1] == 15)
		{
			Color textColor = JudgeNumber switch
			{
				9 => Color.Gold,
				8 or 7 => Color.Orange,
				6 or 5 => Color.Yellow,
				4 or 3 => Color.White,
				_ => Color.Gray
			};
			
			CombatText.NewText(Projectile.getRect(), textColor, JudgeNumber.ToString(), true, true);
		}

		// Deal AoE damage at frame 20 (peak of swing)
		if (Projectile.ai[1] == 20 && !hasDealtDamage)
		{
			hasDealtDamage = true;
			
			// Scale AoE based on judge number: 1 = 100px, 9 = 2000px (multiple screens)
			float aoeRadius = 100f + (JudgeNumber * 200f); // 300px to 1900px
			
			// Visual effect for AoE
			int dustCount = JudgeNumber * 15;
			Color dustColor = JudgeNumber == 9 ? Color.Gold : (JudgeNumber >= 7 ? Color.Orange : Color.White);
			
			for (int i = 0; i < dustCount; i++)
			{
				float angle = MathHelper.TwoPi / dustCount * i;
				Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * aoeRadius;
				
				Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, 
					JudgeNumber == 9 ? DustID.GoldFlame : DustID.Torch, 
					Vector2.Zero, 100, dustColor, 2f);
				dust.noGravity = true;
			}
			
			// Deal damage to all players in AoE
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int i = 0; i < Main.maxPlayers; i++)
				{
					Player player = Main.player[i];
					if (player.active && !player.dead && Vector2.Distance(Projectile.Center, player.Center) <= aoeRadius)
					{
						int damage = Projectile.damage;
						
						// Instant kill on 9 for Last Judge
						if (JudgeNumber == 9 && Projectile.damage >= 200)
						{
							player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(player.whoAmI, Projectile.whoAmI), 
								99999, 0);
						}
						else
						{
							player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(player.whoAmI, Projectile.whoAmI), 
								damage, 0);
						}
						
						// Knockback based on judge number
						if (JudgeNumber >= 7)
						{
							player.velocity.X += 10f * Math.Sign(player.Center.X - Projectile.Center.X);
							player.velocity.Y -= 8f;
						}
						else if (JudgeNumber >= 4)
						{
							player.velocity.X += 5f * Math.Sign(player.Center.X - Projectile.Center.X);
							player.velocity.Y -= 4f;
						}
					}
				}
			}
		}

		// Visual effects during swing
		if (JudgeNumber == 9)
		{
			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
					DustID.GoldFlame, 0f, 0f, 100, default, 1.5f);
				dust.noGravity = true;
			}
		}
		else if (JudgeNumber >= 7)
		{
			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
					DustID.Torch, 0f, 0f, 100, default, 1.2f);
				dust.noGravity = true;
			}
		}
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo info)
	{
		// Instant kill on 9 (for Last Judge attack)
		if (JudgeNumber == 9 && Projectile.damage >= 200) // High damage indicates Last Judge (30*9 = 270)
		{
			target.KillMe(Terraria.DataStructures.PlayerDeathReason.ByProjectile(target.whoAmI, Projectile.whoAmI), 99999, 0);
			
			// Epic effect
			for (int i = 0; i < 50; i++)
			{
				Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, 
					DustID.GoldFlame, 0f, 0f, 100, default, 3f);
				dust.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 15f);
				dust.noGravity = true;
			}
		}
		
		// Apply stronger knockback on higher numbers
		if (JudgeNumber >= 7)
		{
			target.velocity.X += 10f * Projectile.direction;
			target.velocity.Y -= 8f;
		}
		else if (JudgeNumber >= 4)
		{
			target.velocity.X += 5f * Projectile.direction;
			target.velocity.Y -= 4f;
		}
	}		public override Color? GetAlpha(Color lightColor)
		{
			// Special color for 9
			if (JudgeNumber == 9)
			{
				return Color.Gold;
			}
			return null;
		}
	}
}

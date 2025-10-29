using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace VokerrsBosses.Content.Projectiles.Bosses.MrGameAndWatch
{
	public class JudgeHammer : ModProjectile
	{
		private int JudgeNumber => (int)Projectile.ai[0];

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Judge Hammer");
		}

		public override void SetDefaults()
		{
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 30;
			Projectile.aiStyle = -1;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			// Follow the NPC owner
			if (Projectile.owner == 255)
			{
				// Find Mr. Game and Watch
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
			}

		// Swing animation
		Projectile.rotation = MathHelper.Lerp(-0.5f, 0.5f, Projectile.ai[1] / 30f) * Projectile.direction;
		Projectile.ai[1]++;

		// Spawn number above hammer
		if (Projectile.ai[1] == 15) // Show number halfway through swing
		{
			// Create floating combat text showing the judge number
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

		// Critical hit on 9
		if (JudgeNumber == 9)
		{
			// Extra effects for the legendary 9
			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
					DustID.GoldFlame, 0f, 0f, 100, default, 1.5f);
				dust.noGravity = true;
			}
		}
		else if (JudgeNumber >= 7)
		{
			// Strong hits get orange sparks
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

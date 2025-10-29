using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace VokerrsBosses.Content.Projectiles.Weapons
{
	public class PlayerJudgeHammer : ModProjectile
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
			Projectile.friendly = true; // Damages enemies
			Projectile.hostile = false; // Does NOT damage players
			Projectile.penetrate = -1;
			Projectile.timeLeft = 45;
			Projectile.aiStyle = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override void AI()
		{
			// Follow the player owner during windup
			if (Projectile.ai[1] < 20)
			{
				Player owner = Main.player[Projectile.owner];
				if (owner.active)
				{
					Projectile.Center = owner.Center + new Vector2(owner.direction * 30, 0);
					Projectile.direction = owner.direction;
					Projectile.spriteDirection = owner.direction;
				}
			}

			// Swing animation
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
				
				// Scale AoE based on judge number: 1 = 300px, 9 = 1900px
				float aoeRadius = 100f + (JudgeNumber * 200f);
				
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
				
				// Deal damage to all NPCs in AoE
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int i = 0; i < Main.maxNPCs; i++)
					{
						NPC npc = Main.npc[i];
						if (npc.active && !npc.friendly && Vector2.Distance(Projectile.Center, npc.Center) <= aoeRadius)
						{
							int damage = Projectile.damage;
							float knockback = Projectile.knockBack;
							
							// Extra knockback for higher rolls
							if (JudgeNumber >= 7)
							{
								knockback *= 2f;
							}
							else if (JudgeNumber >= 4)
							{
								knockback *= 1.5f;
							}
							
							// Calculate hit info and strike NPC
							NPC.HitInfo hitInfo = new NPC.HitInfo
							{
								Damage = damage,
								Knockback = knockback,
								HitDirection = Math.Sign(npc.Center.X - Projectile.Center.X)
							};
							
							npc.StrikeNPC(hitInfo);
							
							// Sync in multiplayer
							if (Main.netMode != NetmodeID.SinglePlayer)
							{
								NetMessage.SendStrikeNPC(npc, hitInfo);
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

		public override Color? GetAlpha(Color lightColor)
		{
			// Special color for 9
			if (JudgeNumber == 9)
			{
				return Color.Gold;
			}
			return null;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// Use the same sprite as the boss version
			Texture2D texture = ModContent.Request<Texture2D>("VokerrsBosses/Content/Projectiles/Bosses/MrGameAndWatch/JudgeHammer").Value;
			
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
			Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
			
			SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			
			Main.EntitySpriteDraw(texture, drawPosition, sourceRect, Projectile.GetAlpha(lightColor), 
				Projectile.rotation, origin, Projectile.scale, effects, 0);
			
			return false; // Don't draw the default sprite
		}
	}
}

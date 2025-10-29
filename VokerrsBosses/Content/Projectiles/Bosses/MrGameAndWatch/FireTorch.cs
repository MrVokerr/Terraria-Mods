using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VokerrsBosses.Content.Projectiles.Bosses.MrGameAndWatch
{
	public class FireTorch : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Fire Torch");
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 150;
			Projectile.aiStyle = -1;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			// Light
			Lighting.AddLight(Projectile.Center, 0.9f, 0.5f, 0.1f);

			// Rotation based on velocity
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			// Fire particles
			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
					DustID.Torch, 0f, 0f, 100, default, 1.5f);
				dust.velocity *= 0.5f;
				dust.noGravity = true;
			}

			// Slight homing towards players
			Projectile.ai[0]++;
			if (Projectile.ai[0] > 20)
			{
				float homingRange = 200f;
				Player target = null;
				float minDist = homingRange;

				for (int i = 0; i < Main.maxPlayers; i++)
				{
					Player player = Main.player[i];
					if (player.active && !player.dead)
					{
						float dist = Vector2.Distance(Projectile.Center, player.Center);
						if (dist < minDist)
						{
							minDist = dist;
							target = player;
						}
					}
				}

				if (target != null)
				{
					Vector2 direction = target.Center - Projectile.Center;
					direction.Normalize();
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 6f, 0.02f);
				}
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			// Apply on fire debuff
			target.AddBuff(BuffID.OnFire, 180);
		}

		public override void OnKill(int timeLeft)
		{
			// Fire explosion effect
			for (int i = 0; i < 20; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
					DustID.Torch, 0f, 0f, 100, default, 2f);
				dust.velocity *= 2f;
				dust.noGravity = true;
			}
		}
	}
}

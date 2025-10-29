using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VokerrsBosses.Content.Projectiles.Bosses.MrGameAndWatch
{
	public class ChefSausage : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Sausage");
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 180;
			Projectile.aiStyle = -1;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			// Gravity
			Projectile.velocity.Y += 0.3f;
			if (Projectile.velocity.Y > 16f)
			{
				Projectile.velocity.Y = 16f;
			}

			// Rotation
			Projectile.rotation += 0.2f * Projectile.direction;

			// Dust effect
			if (Main.rand.NextBool(5))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
					DustID.Smoke, 0f, 0f, 100, default, 0.8f);
				dust.velocity *= 0.3f;
				dust.noGravity = true;
			}
		}

		public override void OnKill(int timeLeft)
		{
			// Spawn dust on impact
			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
					DustID.Smoke, 0f, 0f, 100, default, 1.2f);
				dust.velocity *= 1.5f;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			// Bounce effect
			if (Projectile.velocity.X != oldVelocity.X)
			{
				Projectile.velocity.X = -oldVelocity.X * 0.5f;
			}
			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				Projectile.velocity.Y = -oldVelocity.Y * 0.5f;
			}

			// Die after bouncing a few times
			Projectile.ai[0]++;
			if (Projectile.ai[0] >= 3)
			{
				return true;
			}

			return false;
		}
	}
}

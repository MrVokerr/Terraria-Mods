using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VokerrsBosses.Content.Projectiles.Bosses.MrGameAndWatch
{
	public class OilBlast : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Oil Blast");
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 120;
			Projectile.aiStyle = -1;
			Projectile.tileCollide = true;
			Projectile.alpha = 50;
		}

		public override void AI()
		{
			// Slow down over time
			Projectile.velocity *= 0.98f;

			// Expand size
			Projectile.scale += 0.01f;
			if (Projectile.scale > 1.5f)
			{
				Projectile.scale = 1.5f;
			}

			// Rotation
			Projectile.rotation += 0.1f;

			// Oil drip effect
			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
					DustID.Smoke, 0f, 0f, 100, Color.Black, 1.2f);
				dust.velocity.Y += 1f;
				dust.noGravity = true;
			}

			// Fade out near end
			if (Projectile.timeLeft < 30)
			{
				Projectile.alpha += 8;
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			// Apply oiled debuff (slippery movement)
			target.AddBuff(BuffID.Slimed, 180);
		}

		public override void OnKill(int timeLeft)
		{
			// Oil splash effect
			for (int i = 0; i < 15; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
					DustID.Smoke, 0f, 0f, 100, Color.Black, 1.5f);
				dust.velocity *= 2f;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			// Splash on tile collision
			Projectile.penetrate--;
			if (Projectile.penetrate <= 0)
			{
				Projectile.Kill();
				return true;
			}

			// Bounce
			if (Projectile.velocity.X != oldVelocity.X)
			{
				Projectile.velocity.X = -oldVelocity.X * 0.6f;
			}
			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				Projectile.velocity.Y = -oldVelocity.Y * 0.6f;
			}

			return false;
		}
	}
}

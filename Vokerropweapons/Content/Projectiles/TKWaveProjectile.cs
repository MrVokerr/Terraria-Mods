using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;

namespace Vokerropweapons.Content.Projectiles
{
    public class TKWaveProjectile : ModProjectile
    {
        private Vector2 baseVelocity;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;

            // ✅ Multiple projectiles hit large targets
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                baseVelocity = Projectile.velocity;
                Projectile.ai[0] = 1;
            }

            // Apply constant straight motion
            Projectile.position += baseVelocity;

            // Delay wave effect for 1 second (60 ticks * extraUpdates = 120 updates)
            int waveStartTime = 30;

            if (Projectile.localAI[0] >= waveStartTime)
            {
                // Smooth wave after delay
                float frequency = (float)(2 * Math.PI / 60f); // 2 cycles per second
                float amplitude = 6f; 

                float waveOffset = (float)Math.Sin((Projectile.localAI[0] - waveStartTime) * frequency) * amplitude;
                Vector2 offset = new Vector2(0f, waveOffset);
                Projectile.position += offset;
            }

            Projectile.localAI[0]++; // Advance timer

            Projectile.rotation = baseVelocity.ToRotation() + MathHelper.PiOver2;

            // Yellow trail effect
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                dust.noGravity = true;
                dust.scale = 1.2f;
                dust.velocity *= 0.1f;
            }
        }
    }
}

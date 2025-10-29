using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Vokerropweapons.Content.Projectiles.Minions
{
    public class HumanoidBolt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // The sprite has 4 vertical frames (provided by the user). Set that up so the engine animates correctly.
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false; // Pass through tiles
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Pinkish light
            Lighting.AddLight(Projectile.Center, 0.35f, 0.08f, 0.25f);

            // Animate frames (4-frame vertical sprite)
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            // Rotate to face travel direction
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = 0.9f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Small hit effect (pink shards)
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch);
                d.velocity *= 0.6f;
                d.scale = 0.9f;
            }
        }
    }
}

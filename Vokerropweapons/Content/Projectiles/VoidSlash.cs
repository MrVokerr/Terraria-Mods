using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Vokerropweapons.Content.Buffs;

namespace Vokerropweapons.Content.Projectiles
{
    public class VoidSlash : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.aiStyle = 27;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60;
        }

        public override void AI()
        {
            // Purple void-like light
            Lighting.AddLight(Projectile.Center, 0.3f, 0f, 0.4f);

            // Shadowflame-style dust trail
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);

            // Slow rotation for visual flair
            Projectile.rotation += 0.2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 30% chance to apply Voidburn
            if (Main.rand.NextFloat() < 0.3f)
            {
                target.AddBuff(ModContent.BuffType<Voidburn>(), 180);
            }

            // Create explosion when enemy dies
            if (target.life <= 0 || !target.active)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_Death(),
                    target.Center,
                    Vector2.Zero,
                    ProjectileID.ShadowBeamHostile,
                    damageDone / 2,
                    5f,
                    Projectile.owner
                );
            }
        }
    }
}

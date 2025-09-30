using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Vokerropweapons.Content.Buffs;

namespace Vokerropweapons.Content.Projectiles.Minions
{
    public class BeanMinion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // Display name & minion classification
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 1; // If you want animation, increase frames
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Kill projectile if player is dead or doesn't have the buff
            if (!player.active || player.dead || !player.HasBuff(ModContent.BuffType<BeanMinionBuff>()))
            {
                Projectile.Kill();
                return;
            }

            if (player.HasBuff(ModContent.BuffType<BeanMinionBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            // Targeting logic
            NPC target = FindTarget();
            float speed = 8f;

            if (target != null)
            {
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                Projectile.velocity = (Projectile.velocity * 20f + direction * speed) / 21f;
            }
            else
            {
                // Idle movement when no target is found
                Vector2 idlePosition = player.Center + new Vector2(0f, -60f);
                Vector2 toIdle = idlePosition - Projectile.Center;

                if (toIdle.Length() > 200f)
                {
                    toIdle.Normalize();
                    toIdle *= speed;
                    Projectile.velocity = (Projectile.velocity * 40f + toIdle) / 41f;
                }
                else if (Projectile.velocity.Length() < 1f)
                {
                    // Add a bit of motion to prevent freezing in place
                    Projectile.velocity.X += Main.rand.NextFloat(-0.1f, 0.1f);
                    Projectile.velocity.Y += Main.rand.NextFloat(-0.1f, 0.1f);
                }
            }

            // Rotate slightly based on movement direction
            Projectile.rotation = Projectile.velocity.X * 0.05f;
        }

        private NPC FindTarget()
        {
            float maxDistance = 800f;
            NPC selectedTarget = null;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy(this, false))
                {
                    float distance = Vector2.Distance(npc.Center, Projectile.Center);
                    if (distance < maxDistance)
                    {
                        maxDistance = distance;
                        selectedTarget = npc;
                    }
                }
            }

            return selectedTarget;
        }
    }
}

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Vokerropweapons.Content.Buffs;

namespace Vokerropweapons.Content.Projectiles.Minions
{
    public class BeanMinion : ModProjectile
    {
        private const bool HYPER_TRACKING = false; // flip true for extreme tracking

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000; // required for correct spawn behavior
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

            // Normal minion lifecycle — die if player dead or buff gone
            if (!player.active || player.dead || !player.HasBuff(ModContent.BuffType<BeanMinionBuff>()))
            {
                Projectile.Kill();
                return;
            }

            // Keep alive indefinitely while buff exists
            Projectile.timeLeft = 2;

            // Movement tuning
            float baseSpeed = HYPER_TRACKING ? 20f : 16f;
            float inertia = HYPER_TRACKING ? 8f : 10f;

            NPC target = FindTarget();

            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                float distance = toTarget.Length();
                toTarget.Normalize();

                float speed = baseSpeed;
                if (distance > 800f) speed *= 2f;
                else if (distance > 400f) speed *= 1.5f;

                Projectile.velocity = (Projectile.velocity * (inertia - 1f) + toTarget * speed) / inertia;
            }
            else
            {
                // Idle near player
                Vector2 idlePos = player.Center + new Vector2(0f, -60f);
                Vector2 toIdle = idlePos - Projectile.Center;
                float dist = toIdle.Length();

                float idleSpeed = MathHelper.Max(6f, baseSpeed * 0.75f);
                if (dist > 200f)
                {
                    toIdle.Normalize();
                    toIdle *= idleSpeed;
                    Projectile.velocity = (Projectile.velocity * 40f + toIdle) / 41f;
                }
                else if (Projectile.velocity.Length() < 1f)
                {
                    Projectile.velocity.X += Main.rand.NextFloat(-0.1f, 0.1f);
                    Projectile.velocity.Y += Main.rand.NextFloat(-0.1f, 0.1f);
                }
            }

            Projectile.rotation = Projectile.velocity.X * 0.05f;
        }

        private NPC FindTarget()
        {
            float maxDistance = 1600f; // 🔹 extended aggro range
            NPC selectedTarget = null;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy(this, false))
                {
                    float distance = Vector2.Distance(npc.Center, Projectile.Center);
                    bool isBoss = npc.boss;
                    if (isBoss) distance *= 0.5f; // prioritize bosses slightly

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

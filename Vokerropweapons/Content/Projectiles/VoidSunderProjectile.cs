using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Vokerropweapons.Content.Buffs;

namespace Vokerropweapons.Content.Projectiles
{
    public class VoidSunderProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName handled by localization if desired
            // This projectile uses a 4-frame vertical spritesheet
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1; // smoother movement for fast projectile
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // States:
            // ai[0] == 0 => outgoing
            // ai[0] == 1 => slowing at max distance (wind-up)
            // ai[0] == 2 => returning (fast)
            // ai[1] stores maxDistance when initialized
            // localAI[0] used as timers, localAI[1] used as 'passed player' flag

            if (Projectile.ai[0] == 0f)
            {
                if (Projectile.ai[1] == 0f)
                {
                    Projectile.ai[1] = Math.Max(Main.screenWidth, Main.screenHeight) * 0.6f;
                }

                if (Vector2.Distance(Projectile.Center, player.Center) >= Projectile.ai[1])
                {
                    // Enter slowing/wind-up state
                    Projectile.ai[0] = 1f;
                    Projectile.localAI[0] = 0f; // timer
                    // Slow down dramatically to emphasize the wind-up
                    Projectile.velocity *= 0.25f;
                    Projectile.netUpdate = true;
                }
            }

            // Wind-up: slow a bit for a short time, then launch back quickly
            if (Projectile.ai[0] == 1f)
            {
                Projectile.localAI[0] += 1f;
                // Make it slowly drift while wind-up
                Projectile.velocity *= 0.98f;

                if (Projectile.localAI[0] >= 10f)
                {
                    // Start fast return
                    Projectile.ai[0] = 2f;
                    Projectile.localAI[0] = 0f;
                    Projectile.localAI[1] = 0f; // not passed player yet
                    // Give a strong velocity toward the player
                    Vector2 toPlayer = player.Center - Projectile.Center;
                    if (toPlayer == Vector2.Zero)
                        toPlayer = new Vector2(player.direction, 0);
                    toPlayer.Normalize();
                    Projectile.velocity = toPlayer * 36f; // very fast return
                    Projectile.netUpdate = true;
                }
            }

            // Returning phase: fast homing toward player, will pass and keep going
            if (Projectile.ai[0] == 2f)
            {
                Vector2 toPlayer = player.Center - Projectile.Center;
                float dist = toPlayer.Length();
                float returnSpeed = 36f;

                if (dist > 1f)
                {
                    toPlayer.Normalize();
                    // Maintain a fast return but allow slight homing adjustments
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * returnSpeed, 0.12f);
                }

                // Detect passing the player: if velocity points away from the vector to the player, we've passed
                float dot = Vector2.Dot(Projectile.velocity, toPlayer);
                if (dot < 0f && Projectile.localAI[1] == 0f)
                {
                    Projectile.localAI[1] = 1f; // mark passed
                    Projectile.localAI[0] = 0f; // reuse as post-pass timer
                    Projectile.netUpdate = true;
                }

                if (Projectile.localAI[1] == 1f)
                {
                    Projectile.localAI[0] += 1f;
                    // After passing the player, travel for a short bit more then despawn
                    if (Projectile.localAI[0] >= 20f)
                    {
                        Projectile.Kill();
                        return;
                    }
                }
            }

            // Visuals
            Lighting.AddLight(Projectile.Center, 0.25f, 0f, 0.35f);
            if (Main.rand.NextBool(2))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);

            // Animate the 4-frame sprite
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Shouldn't collide with tiles (always pass through)
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Voidburn occasionally
            if (Main.rand.NextFloat() < 0.25f)
            {
                target.AddBuff(ModContent.BuffType<Voidburn>(), 120);
            }
        }
    }
}

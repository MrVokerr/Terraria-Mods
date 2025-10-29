using System;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Vokerropweapons.Content.Projectiles.Minions
{
    public class HumanoidMinion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            // The humanoid sprite sheet supplied has 23 frames stacked vertically.
            Main.projFrames[Projectile.type] = 23;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 3f; // Uses 3 minion slots
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

            // Die if player dead or buff removed
            if (!player.active || player.dead || !player.HasBuff(ModContent.BuffType<Buffs.HumanoidMinionBuff>()))
            {
                Projectile.Kill();
                return;
            }

            // Keep alive
            Projectile.timeLeft = 2;

            // Determine position based on which minion this is (count existing minions of this type)
            int minionIndex = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == Projectile.type)
                {
                    if (p.whoAmI == Projectile.whoAmI)
                        break; // This is us, stop counting
                    minionIndex++;
                }
            }

            // Position based on index: 0=behind, 1=front, 2=above middle
            Vector2 offset;
            if (minionIndex == 0)
            {
                // First minion: behind the player
                offset = new Vector2(-player.direction * 36f, -40f);
            }
            else if (minionIndex == 1)
            {
                // Second minion: in front of the player (closer)
                offset = new Vector2(player.direction * 36f, -40f);
            }
            else
            {
                // Third minion (or beyond): above middle
                offset = new Vector2(0f, -80f);
            }

            Vector2 idlePos = player.Center + offset;

            // Keep near the player regardless of aggro (turret behavior)
            Vector2 toIdle = idlePos - Projectile.Center;
            float distToIdle = toIdle.Length();
            float idleSpeed = 8f;
            float idleInertia = 18f;

            if (distToIdle > 200f)
            {
                toIdle.Normalize();
                toIdle *= idleSpeed;
                Projectile.velocity = (Projectile.velocity * (idleInertia - 1f) + toIdle) / idleInertia;
            }
            else
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle, 0.08f);
            }

            // Determine engage range a bit past the current screen size so it works with zoom mods
            float engageRange = Math.Max(Main.screenWidth, Main.screenHeight) * 1.1f + 400f;

            // Find a nearby target within engageRange
            NPC target = FindTarget(engageRange);

            // Shooting behavior: turret-style (stay near player, shoot at target)
            if (target != null)
            {
                // Aim at target and periodically shoot
                Vector2 toTarget = target.Center - Projectile.Center;
                float distance = toTarget.Length();
                toTarget.Normalize();

                // Cooldown timer in ai[0]
                Projectile.ai[0] += 1f;
                int shootCooldown = 30; // ticks between shots (0.5s)

                if (Projectile.ai[0] >= shootCooldown)
                {
                    Projectile.ai[0] = 0f;

                    if (Projectile.owner == Main.myPlayer)
                    {
                        float shotSpeed = 14f;
                        Vector2 shotVelocity = toTarget * shotSpeed;
                        int projType = ModContent.ProjectileType<Projectiles.Minions.HumanoidBolt>();
                        int damage = Projectile.damage; // inherit minion damage
                        float knockback = 2f;

                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shotVelocity, projType, damage, knockback, Projectile.owner);
                        SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
                        // Trigger attack animation timer (play attack frames)
                        Projectile.localAI[1] = 16f; // duration in ticks for attack animation
                        Projectile.frame = 19; // start of attack frames (frames 20..23 are attack frames, 0-based index 19..22)
                        Projectile.frameCounter = 0;
                    }
                }
            }

            // Keep upright and humanoid (no spin)
            Projectile.rotation = 0f;

            // Sprite direction/flip so it faces away from the player (opposite direction)
            Projectile.spriteDirection = -player.direction;

            // Animation handling
            // Frame mapping (1-based frames described by user converted to 0-based indices):
            // True idle: frame 1 -> index 0
            // Frames 2-5 (indices 1-4) are skipped / unused
            // Walking: frames 6-14 -> indices 5..13
            // Second idle (play occasionally while stationary): frames 14-19 -> indices 13..18
            // Attack: frames 20-23 -> indices 19..22
            int idleFrame = 0;
            int walkStart = 5;
            int walkEnd = 13;
            int secondIdleStart = 13;
            int secondIdleEnd = 18;
            int attackStart = 19;
            int attackEnd = 22;

            Projectile.frameCounter++;

            // Animation state machine
            // localAI[1] used for attack animation timer (set when firing)
            // localAI[0] used for second-idle cooldown/timer

            // Attack animation (highest priority)
            if (Projectile.localAI[1] > 0f)
            {
                Projectile.localAI[1] -= 1f;
                if (Projectile.frameCounter >= 4)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame > attackEnd) Projectile.frame = attackStart;
                }
            }
            else if (target != null)
            {
                // Target present: play alert/shoot pre-frames (we'll reuse walk range as a quick alert if desired)
                if (Projectile.frameCounter >= 6)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    // clamp to walking frames to show activity while targeting
                    if (Projectile.frame > walkEnd) Projectile.frame = walkStart;
                }
            }
            else
            {
                // No target: decide between true idle (single frame), occasional second idle, or walking if far from idle pos
                // If minion is moving significantly (e.g., catching up), play walking animation
                if (Projectile.velocity.Length() > 1.5f || distToIdle > 48f)
                {
                    if (Projectile.frameCounter >= 8)
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame++;
                        if (Projectile.frame > walkEnd) Projectile.frame = walkStart;
                    }
                }
                else
                {
                    // Second idle: occasionally play the second idle animation
                    if (Projectile.localAI[0] > 0f)
                    {
                        // second-idle is active, play its frames
                        Projectile.localAI[0] -= 1f;
                        if (Projectile.frameCounter >= 8)
                        {
                            Projectile.frameCounter = 0;
                            Projectile.frame++;
                            if (Projectile.frame > secondIdleEnd) Projectile.frame = secondIdleStart;
                        }
                    }
                    else
                    {
                        // Default true idle frame
                        Projectile.frame = idleFrame;

                        // Occasionally start the second idle animation (roughly every 8-16s)
                        if (Main.rand.NextBool(480)) // ~8 seconds at 60fps
                        {
                            Projectile.localAI[0] = (secondIdleEnd - secondIdleStart + 1) * 8; // duration in ticks
                            Projectile.frame = secondIdleStart;
                            Projectile.frameCounter = 0;
                        }
                    }
                }
            }
        }

        private NPC FindTarget(float maxDistance)
        {
            Player player = Main.player[Projectile.owner];
            NPC selected = null;

            // Check for manually targeted NPC first (player's minion target)
            int targetNPC = player.MinionAttackTargetNPC;
            if (targetNPC >= 0 && targetNPC < Main.maxNPCs && Main.npc[targetNPC].CanBeChasedBy(this, false))
            {
                return Main.npc[targetNPC];
            }

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy(this, false))
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < maxDistance)
                    {
                        maxDistance = dist;
                        selected = npc;
                    }
                }
            }

            return selected;
        }
    }
}

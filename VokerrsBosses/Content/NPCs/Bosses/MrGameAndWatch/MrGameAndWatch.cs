using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using VokerrsBosses.Content.Projectiles.Bosses.MrGameAndWatch;

namespace VokerrsBosses.Content.NPCs.Bosses.MrGameAndWatch
{
	[AutoloadBossHead]
	public class MrGameAndWatch : ModNPC
	{
	// AI States
	private enum AIState
	{
		Idle = 0,
		Chef = 1,           // Tosses sausages
		Judge = 2,          // Hammer attack with random damage
		OilPanic = 3,       // Bucket defense then release
		FireAttack = 4,     // Torch attack
		Parachute = 5,      // Jump and float down
		LastJudge = 6       // Desperation attack at 1 HP
	}

	// AI Variables
	private AIState State
	{
		get => (AIState)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float AttackTimer => ref NPC.ai[1];
	private ref float AttackCounter => ref NPC.ai[2];
	private ref float PhaseTimer => ref NPC.ai[3];
	
	// Custom local variables
	private bool hasUsedLastJudge = false;
	private int lastHealthThreshold = 100; // Track health % for judge attacks (starts at 100%)
	private int spawnInvulnTimer = 0; // Spawn invulnerability timer
	
	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 6; // Animation frames
		
		// Boss bar
		NPCID.Sets.MPAllowedEnemies[Type] = true;
		NPCID.Sets.BossBestiaryPriority.Add(Type);
		
		// Disable platforms during boss fight (like Deerclops)
		NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
		NPCID.Sets.DontDoHardmodeScaling[Type] = true;
		NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			CustomTexturePath = "VokerrsBosses/Content/NPCs/Bosses/MrGameAndWatch/MrGameAndWatch_Preview"
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

		// Debuff immunities
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
	}
	public override void SetDefaults()
	{
		NPC.width = 48;
		NPC.height = 64;
		NPC.damage = 100;  // Base contact damage (Duke Fishron tier)
		NPC.defense = 50;  // Same as Duke Fishron
		NPC.lifeMax = 50000;  // Duke Fishron tier (slightly lower for single-phase boss)
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.knockBackResist = 0f;
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		NPC.value = Item.buyPrice(gold: 15);  // More valuable for hardmode-tier boss
		NPC.SpawnWithHigherTime(30);
		NPC.boss = true;
		NPC.npcSlots = 15f;  // Higher threat level
		NPC.aiStyle = -1; // Custom AI
		
		// Boss ignores platforms (falls through them)
		NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Velocity = 1f
		};
		
		if (!Main.dedServ)
		{
			Music = MusicID.Boss3; // Hardmode boss music
		}
	}		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("A mysterious 2D fighter from another world. His flat body and unpredictable attacks make him a formidable foe!")
			});
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			// Boss bag for expert mode
			npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<Items.BossBags.MrGameAndWatchBag>()));
			
		// Normal mode drops
		LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
		
		// Judge's Hammer weapon - guaranteed drop in normal mode
		notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Items.Weapons.Melee.JudgeHammerWeapon>(), 1, 1, 1));
		
		// Trophy (always drops in normal mode, 10% in expert via bag)
		npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Placeables.MrGameAndWatchTrophy>(), 10));
		
		npcLoot.Add(notExpertRule);
	}

	public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
	{
		// Prevent overkill - if damage would bring boss below 5% HP and Last Judge hasn't triggered yet
		if (!hasUsedLastJudge)
		{
			int fivePercentHP = NPC.lifeMax / 20; // 5% of max HP
			int damageAmount = (int)modifiers.FinalDamage.Base;
			
			if (NPC.life - damageAmount < fivePercentHP)
			{
				// Cap the damage so boss survives at exactly 5% HP
				modifiers.SetMaxDamage(NPC.life - fivePercentHP);
			}
		}
	}		public override void AI()
		{
			// Boss ignores platforms - falls through them like player does
			NPC.noTileCollide = false; // Still collide with solid tiles
			
			// Check if standing on platform and force fall through
			int tileX = (int)(NPC.position.X + NPC.width / 2) / 16;
			int tileY = (int)(NPC.position.Y + NPC.height + 4) / 16;
			if (tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY)
			{
				Tile tile = Main.tile[tileX, tileY];
				if (tile.HasTile && TileID.Sets.Platforms[tile.TileType])
				{
					// Standing on platform - phase through it
					NPC.noTileCollide = true;
				}
			}
			
			// Spawn invulnerability - give player 3 seconds to react (180 ticks)
			if (spawnInvulnTimer < 180)
			{
				spawnInvulnTimer++;
				NPC.dontTakeDamage = true;
				
				// Visual indicator - pulse effect
				if (spawnInvulnTimer % 15 == 0)
				{
					for (int i = 0; i < 10; i++)
					{
						Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 
							DustID.MagicMirror, 0f, 0f, 100, default, 1.5f);
						dust.noGravity = true;
						dust.velocity *= 0.5f;
					}
				}
				
				// Don't do anything else during spawn invulnerability
				return;
			}
			else if (spawnInvulnTimer == 180)
			{
				// Remove spawn invulnerability
				NPC.dontTakeDamage = false;
				spawnInvulnTimer++;
				
				// Visual + audio cue that boss is now vulnerable
				SoundEngine.PlaySound(SoundID.MaxMana, NPC.Center);
				for (int i = 0; i < 30; i++)
				{
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 
						DustID.Electric, 0f, 0f, 100, default, 2f);
					dust.noGravity = true;
					dust.velocity = Main.rand.NextVector2Unit() * 4f;
				}
			}
			
			// Targeting
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
			{
				NPC.TargetClosest();
			}

			Player player = Main.player[NPC.target];

			// Despawn if player is too far or dead
			if (!player.active || player.dead)
			{
				NPC.velocity.Y -= 0.4f;
				if (NPC.timeLeft > 10)
				{
					NPC.timeLeft = 10;
				}
				return;
			}

			// Keep player in combat
			if (NPC.Distance(player.Center) > 2500f)
			{
				Vector2 direction = player.Center - NPC.Center;
				direction.Normalize();
				NPC.velocity = direction * 12f;
			}

		PhaseTimer++;

		// Check for Last Judge desperation attack - triggers at 5% HP
		int fivePercentHP = NPC.lifeMax / 20; // 5% of max HP (2500 HP for 50000 max)
		if (NPC.life <= fivePercentHP && !hasUsedLastJudge)
		{
			hasUsedLastJudge = true;
			NPC.life = fivePercentHP; // Keep at 5% HP
			NPC.dontTakeDamage = true; // Invulnerable during Last Judge
			State = AIState.LastJudge;
			AttackTimer = 0;
			NPC.netUpdate = true;				// Dramatic sound effect
				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
				
				// Visual indicator - lots of dust
				for (int i = 0; i < 50; i++)
				{
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 
						DustID.GoldFlame, 0f, 0f, 100, default, 2f);
					dust.velocity *= 3f;
					dust.noGravity = true;
				}
				
				return;
			}

			// AI State Machine
			switch (State)
			{
				case AIState.Idle:
					DoIdle(player);
					break;
				case AIState.Chef:
					DoChefAttack(player);
					break;
				case AIState.Judge:
					DoJudgeAttack(player);
					break;
				case AIState.OilPanic:
					DoOilPanicAttack(player);
					break;
			case AIState.FireAttack:
				DoFireAttack(player);
				break;
			case AIState.Parachute:
				DoParachuteAttack(player);
				break;
			case AIState.LastJudge:
				DoLastJudgeAttack(player);
				break;
		}
	}		private void DoIdle(Player player)
		{
			AttackTimer++;

			// Aggressively move towards player to stay close
			Vector2 direction = player.Center - NPC.Center;
			float distance = direction.Length();
			direction.Normalize();
			
			// Stay within 300 pixels of player - move faster if too far
			float targetSpeed = distance > 300f ? 9f : 7f;
			
			if (NPC.velocity.X < direction.X * targetSpeed)
			{
				NPC.velocity.X += 0.6f;
			}
			else if (NPC.velocity.X > direction.X * targetSpeed)
			{
				NPC.velocity.X -= 0.6f;
			}
			
		// Jump towards player if they're above boss (only if player is significantly above)
		float verticalDistance = player.Center.Y - NPC.Center.Y;
		if (verticalDistance < -200 && NPC.velocity.Y == 0 && AttackTimer % 90 == 0) // Player is significantly above
		{
			NPC.velocity.Y = -12f; // Strong jump upward
			NPC.velocity.X = direction.X * 8f; // Jump towards player horizontally
		}
		// Only do regular small jumps if player is not too far above (prevents jumping away from player)
		else if (AttackTimer % 120 == 0 && NPC.velocity.Y == 0 && verticalDistance > -100) // Regular jumps - much less frequent, only if player not above
		{
			NPC.velocity.Y = -6f; // Smaller jump
		}
		
		// Aggressive dash towards player to close distance
		if (AttackTimer % 50 == 0 && distance > 200f)
		{
			NPC.velocity.X = direction.X * 10f; // Faster dash when far
		}
		
		// Check for health-based Judge attack (only at 80%, 60%, 40%, 20% HP)
		int currentHealthPercent = (NPC.life * 100) / NPC.lifeMax;
		
		// Only trigger Judge at exact thresholds: 80%, 60%, 40%, 20%
		if ((currentHealthPercent <= 80 && lastHealthThreshold > 80) ||
		    (currentHealthPercent <= 60 && lastHealthThreshold > 60) ||
		    (currentHealthPercent <= 40 && lastHealthThreshold > 40) ||
		    (currentHealthPercent <= 20 && lastHealthThreshold > 20 && currentHealthPercent > 5))
		{
			lastHealthThreshold = currentHealthPercent;
			State = AIState.Judge;
			AttackTimer = 0;
			AttackCounter = 0;
			NPC.netUpdate = true;
			return;
		}

		// Choose next attack - Judge removed from random rotation (now triggered at 80%, 60%, 40%, 20% HP thresholds)
		if (AttackTimer > 90)
		{
			AttackTimer = 0;
			AttackCounter = 0;
			
			// Attack selection without Judge (now triggered by health thresholds)
			int roll = Main.rand.Next(100);
			int attack;
			if (roll < 30) attack = 0; // 30% Chef
			else if (roll < 55) attack = 2; // 25% Oil Panic
			else if (roll < 80) attack = 3; // 25% Fire
			else attack = 4; // 20% Parachute
			
			State = (AIState)(attack + 1);
			NPC.netUpdate = true;
		}
		}

	private void DoChefAttack(Player player)
	{
		AttackTimer++;

		// Move side to side while tossing (like a chef at a grill)
		NPC.velocity.X = (float)Math.Sin(AttackTimer * 0.15f) * 2f;
		
		// Occasional small hop
		if (AttackTimer % 30 == 0 && NPC.velocity.Y == 0)
		{
			NPC.velocity.Y = -5f;
		}

		// Toss sausages - carpet bomb the area with wide spread
		if (AttackTimer % 8 == 0 && AttackTimer < 120)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				// Spawn 3 sausages at once with wide horizontal spread for carpet bombing
				for (int i = 0; i < 3; i++)
				{
					// Wide horizontal spread (-8 to 8) for carpet bombing effect
					// High vertical velocity (-22 to -18) so they fly high before raining down
					Vector2 velocity = new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-22f, -18f));
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, 
						ModContent.ProjectileType<ChefSausage>(), 60, 1f);  // Hardmode damage
				}
			}
			SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
		}
		
		if (AttackTimer > 140)
		{
			State = AIState.Idle;
			AttackTimer = 0;
			NPC.netUpdate = true;
		}
	}		private void DoJudgeAttack(Player player)
		{
			AttackTimer++;

			// Dash towards player
			if (AttackTimer == 1)
			{
				Vector2 direction = player.Center - NPC.Center;
				direction.Normalize();
				NPC.velocity = direction * 10f;
				NPC.velocity.Y = -2f;
			}

		// Swing hammer
		if (AttackTimer == 30 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			// Judge has random damage (1-9 in Smash Bros)
			int judgeDamage = Main.rand.Next(1, 10);
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, 
				ModContent.ProjectileType<JudgeHammer>(), judgeDamage * 15, 3f, Main.myPlayer, judgeDamage);  // 15-135 damage range
			
			// Announce the judge number in chat
			string message = judgeDamage switch
			{
				9 => $"Mr. Game & Watch rolled a {judgeDamage}! LEGENDARY!",
				8 or 7 => $"Mr. Game & Watch rolled a {judgeDamage}! Strong hit!",
				6 or 5 => $"Mr. Game & Watch rolled a {judgeDamage}. Decent.",
				4 or 3 => $"Mr. Game & Watch rolled a {judgeDamage}. Weak.",
				_ => $"Mr. Game & Watch rolled a {judgeDamage}... pathetic."
			};
			
			Color messageColor = judgeDamage switch
			{
				9 => new Color(255, 215, 0),      // Gold
				8 or 7 => new Color(255, 165, 0), // Orange
				6 or 5 => Color.Yellow,
				4 or 3 => Color.White,
				_ => Color.Gray
			};
			
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				Main.NewText(message, messageColor.R, messageColor.G, messageColor.B);
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				Terraria.Chat.ChatHelper.BroadcastChatMessage(
					Terraria.Localization.NetworkText.FromLiteral(message), messageColor);
			}
			
			SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
		}			// Slow down
			NPC.velocity *= 0.96f;

			if (AttackTimer > 60)
			{
				State = AIState.Idle;
				AttackTimer = 0;
				NPC.netUpdate = true;
			}
		}

		private void DoOilPanicAttack(Player player)
		{
			AttackTimer++;

			// Hold bucket (defensive)
			NPC.velocity.X *= 0.9f;

			if (AttackTimer < 90)
			{
				// Defense phase - could reduce damage taken
				NPC.defense = 70;  // Boosted defense during Oil Panic
			}
		else if (AttackTimer == 90 && Main.netMode != NetmodeID.MultiplayerClient)
		{
		// Release oil - shoot multiple blobs in a spread pattern like throwing a water bucket
		Vector2 direction = player.Center - NPC.Center;
		direction.Normalize();
		
		// Spawn 5-7 oil blobs in a spread pattern
		int blobCount = Main.rand.Next(5, 8);
		for (int i = 0; i < blobCount; i++)
		{
			// Create spread: rotate the direction by random angles
			float spreadAngle = MathHelper.Lerp(-0.4f, 0.4f, i / (float)(blobCount - 1)) + Main.rand.NextFloat(-0.1f, 0.1f);
			Vector2 velocity = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(10f, 14f);
			
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, 
				ModContent.ProjectileType<OilBlast>(), 80, 2f);  // Hardmode damage
		}
		
		SoundEngine.PlaySound(SoundID.Item96, NPC.Center);
		}			if (AttackTimer > 120)
			{
				NPC.defense = 50; // Reset defense to base value
				State = AIState.Idle;
				AttackTimer = 0;
				NPC.netUpdate = true;
			}
		}

		private void DoFireAttack(Player player)
		{
			AttackTimer++;

			// Move back and forth
			if (AttackTimer < 90)
			{
				NPC.velocity.X = (float)Math.Sin(AttackTimer * 0.1f) * 3f;
			}
			else
			{
				NPC.velocity.X *= 0.95f;
			}

		// Spawn fire projectiles
		if (AttackTimer % 15 == 0 && AttackTimer < 90 && Main.netMode != NetmodeID.MultiplayerClient)
		{
		Vector2 direction = player.Center - NPC.Center;
		direction.Normalize();
		Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 30f, 
			ModContent.ProjectileType<FireTorch>(), 55, 1.5f);  // Hardmode damage - 3x speed (was 10f)				SoundEngine.PlaySound(SoundID.Item34, NPC.Center);
		}			if (AttackTimer > 110)
			{
				State = AIState.Idle;
				AttackTimer = 0;
				NPC.netUpdate = true;
			}
		}

		private void DoParachuteAttack(Player player)
		{
			AttackTimer++;

			// Jump up
			if (AttackTimer == 1)
			{
				NPC.velocity.Y = -12f;
				NPC.noGravity = true;
			}

			// Float down slowly
			if (AttackTimer > 30)
			{
				NPC.velocity.Y = Math.Min(NPC.velocity.Y + 0.05f, 2f);
				
				// Move towards player
				if (NPC.Center.X < player.Center.X)
				{
					NPC.velocity.X += 0.1f;
				}
				else
				{
					NPC.velocity.X -= 0.1f;
				}

				NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -4f, 4f);
			}

			if (AttackTimer > 120 || NPC.velocity.Y == 0)
			{
				NPC.noGravity = false;
				State = AIState.Idle;
				AttackTimer = 0;
				NPC.netUpdate = true;
			}
		}

	private void DoLastJudgeAttack(Player player)
	{
		AttackTimer++;

		// 10 second dramatic buildup with dialogue (540 ticks = 9 seconds at 60 FPS, then hammer)
		if (AttackTimer < 540)
		{
			// Stop moving
			NPC.velocity *= 0.9f;
			
			// Pulse effect every 10 ticks
			if (AttackTimer % 10 == 0)
			{
				for (int i = 0; i < 20; i++)
				{
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 
						DustID.GoldFlame, 0f, 0f, 100, default, 1.5f);
					dust.velocity = Vector2.One.RotatedBy(MathHelper.TwoPi / 20 * i) * 3f;
					dust.noGravity = true;
				}
			}
			
			// Dialogue every 3 seconds (180 ticks) with sound
			if (AttackTimer == 60) // 1 second
			{
				SoundEngine.PlaySound(SoundID.MaxMana, NPC.Center);
				if (Main.netMode == NetmodeID.SinglePlayer)
					Main.NewText("Mr. Game & Watch: *BEEP BEEP*... This is my final gambit!", 255, 215, 0);
				else if (Main.netMode == NetmodeID.Server)
					Terraria.Chat.ChatHelper.BroadcastChatMessage(
						Terraria.Localization.NetworkText.FromLiteral("Mr. Game & Watch: *BEEP BEEP*... This is my final gambit!"),
						new Color(255, 215, 0));
			}
			else if (AttackTimer == 240) // 4 seconds
			{
				SoundEngine.PlaySound(SoundID.MaxMana, NPC.Center);
				if (Main.netMode == NetmodeID.SinglePlayer)
					Main.NewText("Mr. Game & Watch: One swing... one chance... ONE JUDGE!", 255, 215, 0);
				else if (Main.netMode == NetmodeID.Server)
					Terraria.Chat.ChatHelper.BroadcastChatMessage(
						Terraria.Localization.NetworkText.FromLiteral("Mr. Game & Watch: One swing... one chance... ONE JUDGE!"),
						new Color(255, 215, 0));
			}
			else if (AttackTimer == 420) // 7 seconds
			{
				SoundEngine.PlaySound(SoundID.MaxMana, NPC.Center);
				if (Main.netMode == NetmodeID.SinglePlayer)
					Main.NewText("Mr. Game & Watch: *BEEP BEEP BEEP* Rolling the dice of fate... or should I say hammer!", 255, 215, 0);
				else if (Main.netMode == NetmodeID.Server)
					Terraria.Chat.ChatHelper.BroadcastChatMessage(
						Terraria.Localization.NetworkText.FromLiteral("Mr. Game & Watch: *BEEP BEEP BEEP* Rolling the dice of fate... or should I say hammer!"),
						new Color(255, 215, 0));
			}
			
			return;
		}

		// THE FINAL JUDGE at 9 seconds (540 ticks)
		if (AttackTimer == 540)
		{
			// Dash towards player
			Vector2 direction = player.Center - NPC.Center;
			direction.Normalize();
			NPC.velocity = direction * 15f; // Faster than normal
			NPC.velocity.Y = -3f;
			
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
		}

	// Swing hammer at frame 560 (20 ticks after dash)
	if (AttackTimer == 560 && Main.netMode != NetmodeID.MultiplayerClient)
	{
		// Roll the dice of fate
		int judgeDamage = Main.rand.Next(1, 10);
		
		int projID = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, 
			ModContent.ProjectileType<JudgeHammer>(), judgeDamage * 30, 5f, Main.myPlayer, judgeDamage);  // 30-270 damage range for final judge
		
		// Store the judge number for checking later
		if (Main.projectile[projID].ModProjectile is JudgeHammer hammer)
		{
			// Store reference to check if it hits
			NPC.ai[2] = judgeDamage; // Store judge number
		}			// Announce the LAST JUDGE roll
			string message = $"LAST JUDGE! Mr. Game & Watch rolled a {judgeDamage}!";
			Color messageColor = judgeDamage == 9 ? new Color(255, 215, 0) : Color.Red;
			
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				Main.NewText(message, messageColor.R, messageColor.G, messageColor.B);
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				Terraria.Chat.ChatHelper.BroadcastChatMessage(
					Terraria.Localization.NetworkText.FromLiteral(message), messageColor);
			}
			
			SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
			
			// Dramatic effect
			for (int i = 0; i < 30; i++)
			{
				Dust dust = Dust.NewDustDirect(NPC.Center - new Vector2(24, 24), 48, 48, 
					judgeDamage == 9 ? DustID.GoldFlame : DustID.Smoke, 0f, 0f, 100, default, 2f);
				dust.velocity *= 4f;
				dust.noGravity = true;
			}
		}

		// Slow down
		NPC.velocity *= 0.96f;

		// Check result after attack (600 ticks = 10 seconds)
		if (AttackTimer > 600)
			{
				int finalJudge = (int)NPC.ai[2];
				
				if (finalJudge == 9)
				{
					// THE LEGENDARY 9! Boss escapes!
					// Send dramatic message
					if (Main.netMode == NetmodeID.SinglePlayer)
					{
						Main.NewText("Mr. Game & Watch's 9-Hammer connected! He beeps triumphantly and escapes!", 255, 215, 0);
					}
					else if (Main.netMode == NetmodeID.Server)
					{
						Terraria.Chat.ChatHelper.BroadcastChatMessage(
							Terraria.Localization.NetworkText.FromLiteral("Mr. Game & Watch's 9-Hammer connected! He beeps triumphantly and escapes!"),
							new Color(255, 215, 0));
					}
					
					// Flee upwards with style
					NPC.velocity.Y = -20f;
					NPC.noGravity = true;
					NPC.noTileCollide = true;
					
					// Despawn without loot
					NPC.life = 0;
					NPC.checkDead();
					NPC.active = false;
					
					// Epic escape effects
					for (int i = 0; i < 100; i++)
					{
						Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 
							DustID.GoldFlame, 0f, 0f, 100, default, 3f);
						dust.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 15f);
						dust.noGravity = true;
					}
					
				SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);
			}
			else
			{
				// Failed to get the 9 - boss dies with honor
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.NewText("The hammer missed the legendary 9... You lucked out this time!", 200, 200, 200);
				}
				else if (Main.netMode == NetmodeID.Server)
				{
					Terraria.Chat.ChatHelper.BroadcastChatMessage(
						Terraria.Localization.NetworkText.FromLiteral("The hammer missed the legendary 9... You lucked out this time!"),
						new Color(200, 200, 200));
				}
				
				// Remove invulnerability and return to combat
				NPC.dontTakeDamage = false;
				State = AIState.Idle;
				AttackTimer = 0;
				NPC.netUpdate = true;
			}
		}
	}		public override void FindFrame(int frameHeight)
		{
			// Simple animation cycle
			NPC.frameCounter++;
			if (NPC.frameCounter > 8)
			{
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;

				if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[Type])
				{
					NPC.frame.Y = 0;
				}
			}

			// Sprite direction
			if (NPC.velocity.X > 0)
			{
				NPC.spriteDirection = 1;
			}
			else if (NPC.velocity.X < 0)
			{
				NPC.spriteDirection = -1;
			}
		}

		public override bool CheckDead()
		{
			// Death animation or effects could go here
			return true;
		}

		public override void OnKill()
		{
			// Re-enable platforms when boss dies
			// This happens automatically when boss is no longer active
		}
	}
}

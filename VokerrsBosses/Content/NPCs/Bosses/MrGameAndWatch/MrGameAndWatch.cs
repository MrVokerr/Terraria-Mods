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
	private bool hasUsedLastJudge = false;		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 6; // Animation frames
			
			// Boss bar
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.BossBestiaryPriority.Add(Type);

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
			
			// Trophy (always drops in normal mode, 10% in expert via bag)
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Placeables.MrGameAndWatchTrophy>(), 10));
			
			// Placeholder for boss drops - add your custom weapons/items here
			// notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<YourWeaponHere>(), 1, 1, 1));
			
			npcLoot.Add(notExpertRule);
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
		{
			// Prevent overkill - if damage would bring boss below 1 HP and Last Judge hasn't triggered yet
			if (!hasUsedLastJudge && NPC.life <= modifiers.FinalDamage.Base && NPC.life > 1)
			{
				// Cap the damage so boss survives at exactly 1 HP
				modifiers.SetMaxDamage(NPC.life - 1);
			}
		}

		public override void AI()
		{
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

			// Check for Last Judge desperation attack
			if (NPC.life <= 1 && !hasUsedLastJudge)
			{
				hasUsedLastJudge = true;
				NPC.life = 1; // Keep at 1 HP
				NPC.dontTakeDamage = true; // Invulnerable
				State = AIState.LastJudge;
				AttackTimer = 0;
				NPC.netUpdate = true;
				
				// Dramatic sound effect
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

			// Move towards player
			Vector2 direction = player.Center - NPC.Center;
			direction.Normalize();
			
			if (NPC.velocity.X < direction.X * 4f)
			{
				NPC.velocity.X += 0.2f;
			}
			else if (NPC.velocity.X > direction.X * 4f)
			{
				NPC.velocity.X -= 0.2f;
			}

			// Choose next attack
			if (AttackTimer > 90)
			{
				AttackTimer = 0;
				AttackCounter = 0;
				
				// Random attack selection
				int attack = Main.rand.Next(5);
				State = (AIState)(attack + 1);
				NPC.netUpdate = true;
			}
		}

		private void DoChefAttack(Player player)
		{
			AttackTimer++;

			// Slow down
			NPC.velocity.X *= 0.95f;

		// Toss sausages
		if (AttackTimer % 20 == 0 && AttackTimer < 120)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Vector2 velocity = new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-6f, -2f));
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, 
					ModContent.ProjectileType<ChefSausage>(), 60, 1f);  // Hardmode damage
			}
			SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
		}			if (AttackTimer > 140)
			{
				State = AIState.Idle;
				AttackTimer = 0;
				NPC.netUpdate = true;
			}
		}

		private void DoJudgeAttack(Player player)
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
				// Release oil
				Vector2 direction = player.Center - NPC.Center;
				direction.Normalize();
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 8f, 
					ModContent.ProjectileType<OilBlast>(), 80, 2f);  // Hardmode damage
				
				SoundEngine.PlaySound(SoundID.Item96, NPC.Center);
			}

			if (AttackTimer > 120)
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
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 6f, 
					ModContent.ProjectileType<FireTorch>(), 55, 1.5f);  // Hardmode damage
				
				SoundEngine.PlaySound(SoundID.Item34, NPC.Center);
			}

			if (AttackTimer > 110)
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

			// Dramatic buildup
			if (AttackTimer < 60)
			{
				// Stop moving
				NPC.velocity *= 0.9f;
				
				// Pulse effect
				if (AttackTimer % 10 == 0)
				{
					SoundEngine.PlaySound(SoundID.MaxMana, NPC.Center);
					for (int i = 0; i < 20; i++)
					{
						Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 
							DustID.GoldFlame, 0f, 0f, 100, default, 1.5f);
						dust.velocity = Vector2.One.RotatedBy(MathHelper.TwoPi / 20 * i) * 3f;
						dust.noGravity = true;
					}
				}
				return;
			}

			// THE FINAL JUDGE
			if (AttackTimer == 60)
			{
				// Dash towards player
				Vector2 direction = player.Center - NPC.Center;
				direction.Normalize();
				NPC.velocity = direction * 15f; // Faster than normal
				NPC.velocity.Y = -3f;
			}

		// Swing hammer at frame 80
		if (AttackTimer == 80 && Main.netMode != NetmodeID.MultiplayerClient)
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
			}
			
			SoundEngine.PlaySound(SoundID.Item1, NPC.Center);				// Dramatic effect
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

			// Check result after attack
			if (AttackTimer > 120)
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
					Main.NewText("The hammer missed the legendary 9... Mr. Game & Watch accepts defeat!", 200, 200, 200);
				}
				else if (Main.netMode == NetmodeID.Server)
				{
					Terraria.Chat.ChatHelper.BroadcastChatMessage(
						Terraria.Localization.NetworkText.FromLiteral("The hammer missed the legendary 9... Mr. Game & Watch accepts defeat!"),
						new Color(200, 200, 200));
				}
				
				// Remove invulnerability and kill the boss properly (drops loot)
				NPC.dontTakeDamage = false;
				NPC.life = 0;
				NPC.checkDead();
				
				// Death effects
				for (int i = 0; i < 50; i++)
				{
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 
						DustID.Smoke, 0f, 0f, 100, default, 2f);
					dust.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 8f);
					dust.noGravity = true;
				}
				
				SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);
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
	}
}
